using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Ascon.Pilot.Common.DataProtection;
using Ascon.Pilot.DataClasses;
using Ascon.Pilot.DataModifier;
using Ascon.Pilot.Transport;

namespace PilotUITesting.Data
{
    /// <summary>Отказ сервера в асинхронном изменении (NotifyChangeAsyncError).</summary>
    public sealed class ServerError
    {
        internal ServerError(Guid identity, DateTime timeUtc, string message)
        {
            Identity = identity;
            TimeUtc = timeUtc;
            Message = message;
        }

        /// <summary>Идентификатор изменения (то, что вернул Apply).</summary>
        public Guid Identity { get; }

        public DateTime TimeUtc { get; }

        public string Message { get; }

        public override string ToString()
        {
            return $"{TimeUtc:HH:mm:ss} изменение {Identity}: {Message}";
        }
    }

    /// <summary>Сервер отклонил изменение, отправленное тестом.</summary>
    public sealed class ServerChangeRejectedException : Exception
    {
        public ServerChangeRejectedException(string what, ServerError error)
            : base($"Сервер Pilot отклонил изменение ({what}): {error.Message}")
        {
            What = what;
            Error = error;
        }

        public string What { get; }

        public ServerError Error { get; }
    }

    /// <summary>
    /// Добавления 0.2: наличие объекта с учётом «заготовок» удалённых объектов, ожидания с TimeSpan, изменения с подтверждением чтением
    /// (…AndWait), окончательное удаление, связи заданий, отказы сервера. Прежние методы по смыслу не менялись.
    /// </summary>
    public sealed partial class PilotServerClient
    {
        private static readonly TimeSpan ServerPoll = TimeSpan.FromMilliseconds(300);
        private readonly object _errorsSync = new object();
        private readonly List<ServerError> _errors = new List<ServerError>();

        /// <summary>Сколько ждать данные сервера в методах …AndWait без таймаута.</summary>
        public TimeSpan DefaultDataTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>Локальное хранилище файлов Server API (нужно для DeletePermanently/MoveToRecycleBin/файлов).</summary>
        public FileSystemStorageProvider Storage => _storage ?? throw new InvalidOperationException("Нет подключения к серверу Pilot.");

        /// <summary>Отказы сервера в изменениях за время подключения.</summary>
        public IReadOnlyList<ServerError> Errors
        {
            get
            {
                lock (_errorsSync)
                {
                    return _errors.ToList();
                }
            }
        }

        public IReadOnlyList<ServerError> ErrorsSince(DateTime utc)
        {
            return Errors.Where(e => e.TimeUtc >= utc).ToList();
        }

        /// <summary>
        /// Объект есть: не null, не удалён окончательно и не «заготовка» (Server API на удалённый/несуществующий id возвращает объект
        /// с TypeId 0; у корней базы TypeId тоже 0 - они считаются существующими).
        /// </summary>
        public static bool Exists(DObject obj)
        {
            if (obj == null || (obj.StateInfo != null && obj.StateInfo.State == ObjectState.DeletedPermanently))
            {
                return false;
            }
            return obj.TypeId != 0 || DObject.RootsIds.Contains(obj.Id);
        }

        public bool Exists(Guid id)
        {
            return Exists(GetObject(id));
        }

        /// <summary>Объект, если он есть (см. <see cref="Exists(DObject)"/>), иначе null.</summary>
        public DObject TryGetExisting(Guid id)
        {
            DObject obj = GetObject(id);
            return Exists(obj) ? obj : null;
        }

        /// <summary>Объект есть и живой (Alive/Frozen), не в корзине.</summary>
        public bool IsAlive(Guid id)
        {
            DObject obj = TryGetExisting(id);
            return obj != null && (obj.StateInfo == null || obj.StateInfo.State == ObjectState.Alive || obj.StateInfo.State == ObjectState.Frozen);
        }

        public bool IsInRecycleBin(Guid id)
        {
            DObject obj = TryGetExisting(id);
            return obj?.StateInfo != null && obj.StateInfo.State == ObjectState.InRecycleBin;
        }

        /// <summary>Ждать объект (есть на сервере и выполняет условие). У таймаута нет значения по умолчанию - чтобы не спорить с прежней перегрузкой в мс.</summary>
        public DObject WaitObject(Guid id, TimeSpan timeout, Func<DObject, bool> condition = null)
        {
            return Poll(() =>
            {
                DObject obj = TryGetExisting(id);
                return obj != null && (condition == null || condition(obj)) ? obj : null;
            }, timeout, $"объект {id} на сервере" + (condition == null ? "" : " с условием"), () => Describe(id));
        }

        /// <summary>Ждать живой дочерний объект типа с условием.</summary>
        public DObject WaitChild(Guid parentId, string typeName, Func<DObject, bool> predicate, TimeSpan timeout)
        {
            return Poll(() => FindChild(parentId, typeName, predicate), timeout, $"дочерний объект типа {typeName} у {parentId}", null);
        }

        /// <summary>Ждать живой объект типа в поддереве (до maxDepth уровней) - например, замечание в папке замечаний документа.</summary>
        public DObject WaitDescendant(Guid rootId, string typeName, Func<DObject, bool> match, TimeSpan? timeout = null, int maxDepth = 4)
        {
            int typeId = TypeByName(typeName).Id;
            return Poll(() =>
            {
                List<Guid> level = new List<Guid> { rootId };
                for (int depth = 0; depth < maxDepth && level.Count > 0; depth++)
                {
                    List<DObject> children = GetObjects(GetObjects(level).Where(Exists).SelectMany(o => o.Children.Select(c => c.ObjectId)))
                        .Where(o => Exists(o) && (o.StateInfo == null || o.StateInfo.State == ObjectState.Alive || o.StateInfo.State == ObjectState.Frozen)).ToList();
                    DObject found = children.FirstOrDefault(o => o.TypeId == typeId && (match == null || match(o)));
                    if (found != null)
                    {
                        return found;
                    }
                    level = children.Select(o => o.Id).ToList();
                }
                return null;
            }, timeout ?? DefaultDataTimeout, $"объект типа {typeName} в поддереве {rootId}", null);
        }

        /// <summary>Ждать значение атрибута (строковое представление).</summary>
        public DObject WaitAttribute(Guid id, string attribute, string expected, TimeSpan? timeout = null)
        {
            return Poll(() =>
            {
                DObject obj = TryGetExisting(id);
                return obj != null && AttrOrNull(obj, attribute) == expected ? obj : null;
            }, timeout ?? DefaultDataTimeout, $"{attribute} = «{expected}» у {id}", () => "сейчас: «" + AttrOrNull(TryGetExisting(id), attribute) + "»");
        }

        /// <summary>Ждать, что объекта нет (удалён окончательно).</summary>
        public void WaitGone(Guid id, TimeSpan? timeout = null)
        {
            Poll(() => Exists(id) ? null : (object)true, timeout ?? DefaultDataTimeout, $"объект {id} удалён с сервера", () => Describe(id));
        }

        /// <summary>
        /// Изменить атрибуты и дождаться, что сервер их принял (чтение обратно). Значение null - удалить атрибут.
        /// Отказ сервера (NotifyChangeAsyncError с идентификатором этого изменения) - <see cref="ServerChangeRejectedException"/> сразу.
        /// </summary>
        public DObject SetAttributesAndWait(Guid id, IDictionary<string, object> attributes, TimeSpan? timeout = null)
        {
            IModifier modifier = CreateModifier();
            IObjectChangeBuilder builder = modifier.EditObject(id);
            Dictionary<string, DValue> expected = new Dictionary<string, DValue>();
            foreach (KeyValuePair<string, object> pair in attributes)
            {
                if (pair.Value == null)
                {
                    builder.RemoveAttribute(pair.Key);
                    expected[pair.Key] = null;
                    continue;
                }
                DValue value = AsSent(ToDValueStrict(pair.Value));
                builder.SetAttribute(pair.Key, value);
                expected[pair.Key] = value;
            }
            if (!modifier.AnyChanges())
            {
                return TryGetExisting(id);
            }
            string what = "атрибуты " + string.Join(", ", attributes.Keys) + " у " + id;
            Guid identity = modifier.Apply();
            return WaitApplied(identity, what, () =>
            {
                DObject obj = TryGetExisting(id);
                if (obj == null)
                {
                    return null;
                }
                foreach (KeyValuePair<string, DValue> pair in expected)
                {
                    obj.Attributes.TryGetValue(pair.Key, out DValue actual);
                    if (pair.Value == null ? actual != null && actual.Value != null : !pair.Value.Equals(actual))
                    {
                        return null;
                    }
                }
                return obj;
            }, timeout, () => Describe(id));
        }

        /// <summary>Установить состояние (атрибут-состояние) и дождаться. Сервер проверяет допустимость перехода машины состояний.</summary>
        public DObject SetStateAndWait(Guid id, string stateAttribute, Guid stateId, TimeSpan? timeout = null)
        {
            return SetAttributesAndWait(id, new Dictionary<string, object> { { stateAttribute, stateId } }, timeout);
        }

        /// <summary>
        /// Добавить файл новым снимком и дождаться его в актуальном снимке - по Id тела файла, созданного этим изменением
        /// (файл с тем же именем и размером из прежнего снимка не подходит).
        /// </summary>
        public DObject AddFileAndWait(Guid id, string filePath, TimeSpan? timeout = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл не найден", filePath);
            }
            string name = Path.GetFileName(filePath);
            IModifier modifier = CreateModifier();
            INFile added = modifier.EditObject(id).CreateSnapshot("UI test").AddFile(new DocumentInfo(filePath), _storage);
            Guid bodyId = added.Dto.Body.Id;
            Guid identity = modifier.Apply();
            return WaitApplied(identity, $"файл {name} (тело {bodyId}) у {id}", () =>
            {
                DObject obj = TryGetExisting(id);
                return obj?.ActualFileSnapshot != null && obj.ActualFileSnapshot.Files.Any(f => f.Body != null && f.Body.Id == bodyId) ? obj : null;
            }, timeout, () => Describe(id));
        }

        public void MoveToRecycleBinAndWait(Guid id, TimeSpan? timeout = null)
        {
            IModifier modifier = CreateModifier();
            modifier.MoveToRecycleBin(id, _storage);
            Guid identity = modifier.Apply();
            WaitApplied(identity, "в корзину " + id, () => IsInRecycleBin(id) ? (object)true : null, timeout, () => Describe(id));
        }

        /// <summary>Удалить объект окончательно и дождаться (замороженный объект сначала размораживается). Объекта уже нет - ничего не делает.</summary>
        public void DeletePermanently(Guid id, TimeSpan? timeout = null)
        {
            DObject obj = TryGetExisting(id);
            if (obj == null)
            {
                return;
            }
            if (obj.StateInfo != null && obj.StateInfo.State == ObjectState.Frozen)
            {
                IModifier unfreeze = CreateModifier();
                unfreeze.EditObject(id).SetState(ObjectState.Alive);
                Guid unfreezeId = unfreeze.Apply();
                WaitApplied(unfreezeId, "разморозка " + id, () => TryGetExisting(id)?.StateInfo?.State == ObjectState.Alive ? (object)true : null, timeout, () => Describe(id));
            }
            IModifier modifier = CreateModifier();
            modifier.DeletePermanently(id, _storage);
            Guid identity = modifier.Apply();
            WaitApplied(identity, "окончательное удаление " + id, () => Exists(id) ? null : (object)true, timeout, () => Describe(id));
        }

        /// <summary>Задания и процессы, к которым приложен объект (связи TaskAttachments).</summary>
        public IReadOnlyList<Guid> TaskAttachments(Guid id)
        {
            DObject obj = TryGetExisting(id);
            return obj == null ? new Guid[0] : obj.Relations.Where(r => r.Type == RelationType.TaskAttachments).Select(r => r.TargetId).Distinct().ToList();
        }

        /// <summary>Предки объекта от родителя к корню.</summary>
        public IReadOnlyList<Guid> AncestorIds(Guid id)
        {
            List<Guid> result = new List<Guid>();
            DObject current = TryGetExisting(id);
            while (current != null && current.ParentId != Guid.Empty && !result.Contains(current.ParentId))
            {
                result.Add(current.ParentId);
                current = TryGetExisting(current.ParentId);
            }
            return result;
        }

        /// <summary>Время создания актуального снимка файлов (UTC) - так же вычисляет annotation_version папки замечаний модуль реестра.</summary>
        public DateTime ActualSnapshotCreated(Guid id)
        {
            DObject obj = TryGetExisting(id) ?? throw new InvalidOperationException($"Объекта {id} нет на сервере.");
            if (obj.ActualFileSnapshot == null || obj.ActualFileSnapshot.Files.Count == 0)
            {
                throw new InvalidOperationException($"У объекта {id} нет файлов в актуальном снимке.");
            }
            return obj.ActualFileSnapshot.Created;
        }

        /// <summary>
        /// Перечитать метаданные (после изменения типов на сервере). Backend SDK кэширует типы при первом обращении и не перечитывает,
        /// поэтому он создаётся заново: иначе CreateObject нового типа падает на проверке typeId.
        /// </summary>
        public void ReloadMetadata()
        {
            _dbInfo = ServerApi.OpenDatabase();
            _metadata = ServerApi.GetMetadata(_dbInfo.MetadataVersion);
            _backend = CreateBackend();
        }

        /// <summary>
        /// Значение атрибута так, как его отправит на сервер ObjectChangeBuilder.SetAttribute (SetToUniversalTimeIfNeeded, SDK 26.1):
        /// дата, кроме DateTime.MaxValue, переводится в UTC. По нему сверяется прочитанное с сервера значение.
        /// </summary>
        internal static DValue AsSent(DValue value)
        {
            if (value.DateValue.HasValue && value.DateValue.Value != DateTime.MaxValue)
            {
                return value.DateValue.Value.ToUniversalTime();
            }
            return value;
        }

        /// <summary>Значение атрибута строкой; атрибута нет - null (в отличие от <see cref="Attr"/>, который возвращает "").</summary>
        public static string AttrOrNull(DObject obj, string name)
        {
            if (obj == null || !obj.Attributes.TryGetValue(name, out DValue value) || value?.Value == null)
            {
                return null;
            }
            return value.Value.ToString();
        }

        /// <summary>Id состояния по системному имени (среди неудалённых). Не единственное - исключение.</summary>
        public Guid StateIdByName(string name)
        {
            MUserState[] states = Metadata.UserStates.Where(s => s.Name == name && !s.IsDeleted).ToArray();
            if (states.Length != 1)
            {
                throw new InvalidOperationException($"Состояние «{name}»: найдено {states.Length} (ожидалось ровно одно).");
            }
            return states[0].Id;
        }

        private void OnChangeAsyncError(Guid identity, ProtoExceptionInfo info)
        {
            string message;
            try
            {
                message = ProtoExceptionInfo.Deserialize(info)?.Message ?? info?.JsonData ?? "(пустой отказ)";
            }
            catch (Exception ex)
            {
                message = "(текст отказа не разобран: " + ex.Message + ") " + info?.JsonData;
            }
            ServerError error = new ServerError(identity, DateTime.UtcNow, message);
            lock (_errorsSync)
            {
                _errors.Add(error);
            }
            PilotLog.Warn("Сервер Pilot отклонил изменение " + identity + ": " + message);
        }

        private T WaitApplied<T>(Guid identity, string what, Func<T> check, TimeSpan? timeout, Func<string> observe) where T : class
        {
            return Poll(() =>
            {
                ServerError rejected;
                lock (_errorsSync)
                {
                    rejected = _errors.FirstOrDefault(e => e.Identity == identity);
                }
                if (rejected != null)
                {
                    throw new ServerChangeRejectedException(what, rejected);
                }
                return check();
            }, timeout ?? DefaultDataTimeout, what + " (изменение " + identity + ")", observe);
        }

        private static T Poll<T>(Func<T> producer, TimeSpan timeout, string description, Func<string> observe) where T : class
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                bool expired = sw.Elapsed >= timeout;
                T result = producer();
                if (result != null)
                {
                    return result;
                }
                if (expired)
                {
                    throw new Uia.PilotWaitTimeoutException(description, timeout, sw.Elapsed, observe?.Invoke(), null);
                }
                Thread.Sleep(ServerPoll);
            }
        }

        private string Describe(Guid id)
        {
            DObject obj = GetObject(id);
            if (obj == null)
            {
                return "объекта нет";
            }
            return $"тип {TypeNameOf(obj)}, состояние {obj.StateInfo?.State.ToString() ?? "-"}, родитель {obj.ParentId}, атрибуты: " +
                   string.Join("; ", obj.Attributes.Select(a => a.Key + "=" + (a.Value?.Value?.ToString() ?? "null")).Take(20));
        }

        /// <summary>Строгое преобразование: неизвестный тип значения - ArgumentException (прежний ToDValue молча брал ToString()).</summary>
        internal static DValue ToDValueStrict(object value)
        {
            switch (value)
            {
                case DValue d:
                    return d;
                case string s:
                    return s;
                case Guid g:
                    return g;
                case int i:
                    return (long)i;
                case long l:
                    return l;
                case double dbl:
                    return dbl;
                case decimal dec:
                    return dec;
                case DateTime dt:
                    return dt;
                case bool b:
                    return b;
                case int[] ints:
                    return ints;
                case string[] strings:
                    return strings;
                default:
                    throw new ArgumentException($"Тип значения атрибута не поддерживается: {value?.GetType().FullName ?? "null"}");
            }
        }
    }
}
