using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using Ascon.Pilot.DataClasses;

namespace PilotUITesting.Data
{
    /// <summary>Объект базы: id и отображаемый заголовок (по заголовку тест находит строку в проводнике).</summary>
    public sealed class ObjectRef
    {
        public ObjectRef(Guid id, string title)
        {
            Id = id;
            Title = title;
        }

        public Guid Id { get; }

        public string Title { get; }

        public override string ToString()
        {
            return $"«{Title}» ({Id})";
        }
    }

    /// <summary>Как убирать песочницу после тестов класса.</summary>
    public enum SandboxCleanup
    {
        /// <summary>Удалить всё окончательно с проверкой (по умолчанию).</summary>
        DeletePermanently,

        /// <summary>Переместить корень песочницы в корзину.</summary>
        RecycleBin,

        /// <summary>Оставить (для разбора упавшего прогона).</summary>
        Keep,
    }

    /// <summary>Типы песочницы (секция "sandbox" в pilot-ui-tests.json): у каждого - имя типа и атрибут заголовка.</summary>
    public sealed class PilotSandboxConfig
    {
        /// <summary>Тип корня песочницы - допустимый в корне базы (проводник показывает корневые объекты сразу).</summary>
        public string RootType { get; set; }

        public string RootTitleAttribute { get; set; }

        public string FolderType { get; set; }

        public string FolderTitleAttribute { get; set; }

        /// <summary>Тип документа (с файлами), допустимый внутри корня и папки.</summary>
        public string DocumentType { get; set; }

        public string DocumentTitleAttribute { get; set; }

        /// <summary>Префикс имени корня: по нему остатки прогонов видно в базе.</summary>
        public string NamePrefix { get; set; } = "UI";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SandboxCleanup Cleanup { get; set; } = SandboxCleanup.DeletePermanently;

        public IReadOnlyList<string> Validate()
        {
            List<string> problems = new List<string>();
            if (string.IsNullOrWhiteSpace(RootType) || string.IsNullOrWhiteSpace(RootTitleAttribute))
            {
                problems.Add("нужны rootType и rootTitleAttribute");
            }
            if (string.IsNullOrWhiteSpace(FolderType) != string.IsNullOrWhiteSpace(FolderTitleAttribute))
            {
                problems.Add("folderType и folderTitleAttribute задаются вместе");
            }
            if (string.IsNullOrWhiteSpace(DocumentType) != string.IsNullOrWhiteSpace(DocumentTitleAttribute))
            {
                problems.Add("documentType и documentTitleAttribute задаются вместе");
            }
            return problems;
        }
    }

    /// <summary>
    /// Песочница тестовых данных через Server API: корень в корне базы «{префикс} {метка} {время}», внутри - папки и документы.
    /// Уборка (Dispose) удаляет окончательно: объекты, взятые на учёт (<see cref="Track"/>), объекты, которые тест удалил из песочницы
    /// в корзину (через интерфейс или MoveToRecycleBin), и всё поддерево корня - снизу вверх, в том числе объекты, созданные тестом
    /// через интерфейс; каждое удаление проверяется. Объект старше песочницы и его предки не удаляются (сервер удалил бы его вместе
    /// с родителем). Ошибки не глушатся: в конце - исключение с перечнем.
    /// Объекты, удалённые в корзину через интерфейс под другим пользователем, видны уборке, только если учётка Server API - администратор
    /// (иначе - предупреждение в журнале).
    /// </summary>
    public sealed class PilotSandbox : IDisposable
    {
        /// <summary>Атрибут объекта в корзине: id родителя, из которого объект удалён (ModifierBase.MoveToRecycleBin, SDK 26.1; так же - удаление в клиенте).</summary>
        internal const string DeleteSourceAttribute = "Delete_source_EFED1D30-E3E2-49E1-BDF3-13C544DADE9F";

        private readonly PilotServerClient _server;
        private readonly PilotSandboxConfig _config;
        private readonly List<Guid> _tracked = new List<Guid>();
        private readonly long _firstChangeset;
        private int _counter;
        private bool _disposed;

        private PilotSandbox(PilotServerClient server, PilotSandboxConfig config, ObjectRef root, DateTime rootCreatedUtc, string runTag, long firstChangeset)
        {
            _server = server;
            _config = config;
            Root = root;
            RootCreatedUtc = rootCreatedUtc;
            RunTag = runTag;
            _firstChangeset = firstChangeset;
        }

        public ObjectRef Root { get; }

        /// <summary>Время создания корня по данным сервера: объекты старше него уборка не трогает (защита от чужих данных).</summary>
        public DateTime RootCreatedUtc { get; }

        /// <summary>Метка прогона «HHmmss» (входит в заголовки <see cref="Unique"/>).</summary>
        public string RunTag { get; }

        public static PilotSandbox Create(PilotServerClient server, PilotSandboxConfig config, string tag)
        {
            if (server == null)
            {
                throw new ArgumentNullException(nameof(server));
            }
            if (config == null)
            {
                throw new InvalidOperationException("Песочница не настроена: добавьте секцию \"sandbox\" в pilot-ui-tests.json (rootType, rootTitleAttribute, documentType, …).");
            }
            IReadOnlyList<string> problems = config.Validate();
            if (problems.Count > 0)
            {
                throw new InvalidOperationException("Секция \"sandbox\" pilot-ui-tests.json: " + string.Join("; ", problems));
            }
            DateTime now = DateTime.Now;
            // первый набор изменений прогона: по наборам изменений уборка находит объекты, удалённые тестом в корзину
            long firstChangeset = (server.ServerApi.GetDatabaseInfo().LastChangeset?.Id ?? 0) + 1;
            string title = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2:yyyyMMdd-HHmmss}", config.NamePrefix, tag, now);
            Guid id = server.CreateObject(PilotServerClient.RootId, config.RootType, new Dictionary<string, object> { { config.RootTitleAttribute, title } });
            DObject root = server.WaitObject(id, server.DefaultDataTimeout);
            PilotLog.Action($"песочница {title} ({id})");
            return new PilotSandbox(server, config, new ObjectRef(id, title), root.Created, now.ToString("HHmmss", CultureInfo.InvariantCulture), firstChangeset);
        }

        /// <summary>Уникальный внутри прогона заголовок: «baseTitle HHmmss-N».</summary>
        public string Unique(string baseTitle)
        {
            return baseTitle + " " + RunTag + "-" + (++_counter).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Папка песочницы (тип folderType); по умолчанию - в корне песочницы.</summary>
        public ObjectRef CreateFolder(string title, ObjectRef parent = null)
        {
            RequireType(_config.FolderType, "folderType");
            Guid id = _server.CreateObject((parent ?? Root).Id, _config.FolderType, new Dictionary<string, object> { { _config.FolderTitleAttribute, title } });
            return new ObjectRef(id, title);
        }

        /// <summary>Документ песочницы (тип documentType); filePath - файл добавляется, к возврату он уже на сервере.</summary>
        public ObjectRef CreateDocument(string title, ObjectRef parent = null, string filePath = null)
        {
            RequireType(_config.DocumentType, "documentType");
            Guid id = _server.CreateObject((parent ?? Root).Id, _config.DocumentType, new Dictionary<string, object> { { _config.DocumentTitleAttribute, title } });
            if (filePath != null)
            {
                _server.AddFileAndWait(id, filePath);
            }
            return new ObjectRef(id, title);
        }

        /// <summary>Объект произвольного типа с атрибутами (к возврату он на сервере).</summary>
        public Guid Create(Guid parentId, string typeName, params (string Name, object Value)[] attributes)
        {
            return _server.CreateObject(parentId, typeName, attributes.ToDictionary(a => a.Name, a => a.Value));
        }

        /// <summary>Взять на учёт объект вне поддерева песочницы (выданное задание, процесс, объект, перенесённый наружу) - его тоже удалит уборка (вместе с поддеревом).</summary>
        public void Track(Guid id)
        {
            if (id != Guid.Empty && !_tracked.Contains(id))
            {
                _tracked.Add(id);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            if (_config.Cleanup == SandboxCleanup.Keep)
            {
                PilotLog.Warn($"Песочница {Root} оставлена в базе (sandbox.cleanup = Keep).");
                return;
            }
            if (_config.Cleanup == SandboxCleanup.RecycleBin)
            {
                _server.MoveToRecycleBinAndWait(Root.Id);
                return;
            }
            List<string> problems = new List<string>();
            Cleanup plan = new Cleanup(this, problems);
            for (int i = _tracked.Count - 1; i >= 0; i--)
            {
                plan.Visit(_tracked[i]);
            }
            plan.Visit(Root.Id);
            plan.AddRecycled(_firstChangeset);
            foreach (Guid id in plan.Order)
            {
                DObject obj = _server.TryGetExisting(id);
                if (obj == null)
                {
                    continue;
                }
                try
                {
                    _server.DeletePermanently(id);
                }
                catch (Exception ex)
                {
                    problems.Add($"{id} ({_server.TypeNameOf(obj)}): {ex.Message}");
                }
            }
            foreach (Guid id in plan.Order.Where(_server.Exists))
            {
                problems.Add($"{id} остался на сервере после уборки");
            }
            if (problems.Count > 0)
            {
                throw new InvalidOperationException($"Уборка песочницы {Root} прошла не полностью (объекты с префиксом «{_config.NamePrefix}» видно в базе): " + string.Join("; ", problems.Distinct()));
            }
            PilotLog.Action($"песочница {Root} удалена ({plan.Order.Count} объектов)");
        }

        /// <summary>
        /// Порядок удаления: поддеревья снизу вверх (дети раньше родителей). Объект старше песочницы не удаляется, и его предки тоже:
        /// окончательное удаление родителя сервер распространяет на всех потомков (DeletedPermanentlyStateProcessor).
        /// </summary>
        private sealed class Cleanup
        {
            private readonly PilotSandbox _sandbox;
            private readonly List<string> _problems;
            private readonly HashSet<Guid> _seen = new HashSet<Guid>();
            private readonly HashSet<Guid> _blocked = new HashSet<Guid>();

            internal Cleanup(PilotSandbox sandbox, List<string> problems)
            {
                _sandbox = sandbox;
                _problems = problems;
            }

            internal List<Guid> Order { get; } = new List<Guid>();

            /// <summary>Обойти поддерево; true - в нём (или в самом объекте) есть объект старше песочницы.</summary>
            internal bool Visit(Guid id)
            {
                if (!_seen.Add(id))
                {
                    return _blocked.Contains(id);
                }
                DObject obj = _sandbox._server.TryGetExisting(id);
                if (obj == null)
                {
                    return false;
                }
                bool protectedInside = false;
                foreach (DChild child in obj.Children)
                {
                    protectedInside |= Visit(child.ObjectId);
                }
                bool older = obj.Created < _sandbox.RootCreatedUtc && id != _sandbox.Root.Id;
                if (older)
                {
                    _problems.Add($"{id} создан раньше песочницы ({obj.Created:u}) - не удалён");
                }
                else if (protectedInside)
                {
                    _problems.Add($"{id} не удалён: в его поддереве объект старше песочницы");
                }
                if (older || protectedInside)
                {
                    _blocked.Add(id);
                    return true;
                }
                Order.Add(id);
                return false;
            }

            /// <summary>
            /// Объекты, удалённые в корзину из уже обойдённых узлов (атрибут Delete_source), - среди изменённых с начала прогона
            /// (GetChangesets: границы включительно). Повтор, пока находятся новые: удалённая папка могла содержать удалённое раньше.
            /// </summary>
            internal void AddRecycled(long firstChangeset)
            {
                PilotServerClient server = _sandbox._server;
                long last = server.ServerApi.GetDatabaseInfo().LastChangeset?.Id ?? 0;
                if (last < firstChangeset)
                {
                    return;
                }
                List<Guid> changed = server.ServerApi.GetChangesets(firstChangeset, last).SelectMany(c => c.Changed).Distinct().ToList();
                List<DObject> recycled = new List<DObject>();
                int hidden = 0;
                for (int i = 0; i < changed.Count; i += 500)
                {
                    foreach (DObject obj in server.GetObjects(changed.Skip(i).Take(500)))
                    {
                        if (PilotServerClient.Exists(obj) && obj.StateInfo != null && obj.StateInfo.State == ObjectState.InRecycleBin)
                        {
                            recycled.Add(obj);
                        }
                        else if (obj.TypeId == 0 && obj.ParentId == Guid.Empty && !DObject.RootsIds.Contains(obj.Id))
                        {
                            // так сервер отдаёт объект без права просмотра (ToForbidden): объект в корзине чужой должности видят только
                            // администраторы и удалившая должность (ObjectSecurity.GetPredefinedAccess, Pilot-Server 26.11)
                            hidden++;
                        }
                    }
                }
                if (hidden > 0 && !(server.DatabaseInfo.Person?.IsAdmin ?? false))
                {
                    PilotLog.Warn($"Уборка песочницы {_sandbox.Root}: {hidden} объектов, изменённых за прогон, сервер вернул учётке Server API «{server.DatabaseInfo.Person?.Login}» " +
                        "без данных (она не администратор: нет права просмотра, в том числе объектов в корзине чужой должности). Удалённые тестом через интерфейс " +
                        "под другим пользователем объекты уборка найти не может - они могут остаться в корзине.");
                }
                bool added = true;
                while (added)
                {
                    added = false;
                    foreach (DObject obj in recycled.Where(o => !_seen.Contains(o.Id)).ToList())
                    {
                        string source = PilotServerClient.AttrOrNull(obj, DeleteSourceAttribute);
                        if (source != null && Guid.TryParse(source, out Guid parent) && _seen.Contains(parent))
                        {
                            PilotLog.Action($"уборка песочницы: {obj.Id} удалён тестом в корзину из {parent} - удаляется окончательно");
                            Visit(obj.Id);
                            added = true;
                        }
                    }
                }
            }
        }

        private static void RequireType(string type, string key)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new InvalidOperationException($"В секции \"sandbox\" не задан {key}.");
            }
        }
    }
}
