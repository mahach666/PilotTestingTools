using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ascon.Pilot.Common.DataProtection;
using Ascon.Pilot.DataClasses;
using Ascon.Pilot.DataModifier;
using Ascon.Pilot.Server.Api;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.Transport;

namespace PilotUITesting.Data
{
    /// <summary>
    /// Тонкая обёртка над Ascon.Pilot.Server.Api для подготовки тестовых данных и проверки результата
    /// в обход клиента: создание/чтение/удаление объектов, атрибуты, состояния, файлы.
    /// </summary>
    public sealed partial class PilotServerClient : IDisposable
    {
        private HttpPilotClient _client;
        private IServerApi _serverApi;
        private IFileArchiveApi _fileApi;
        private Backend _backend;
        private FileSystemStorageProvider _storage;
        private DMetadata _metadata;
        private DDatabaseInfo _dbInfo;
        private string _url;
        private string _login;
        private string _password;
        private int _licenseType;
        private volatile string _lostReason;

        /// <summary>
        /// Сколько раз сеанс восстанавливался после разрыва сервером. Разрыв не скрывается: каждое
        /// переподключение пишется предупреждением в журнал прогона (PilotLog.Warn).
        /// </summary>
        public int ReconnectCount { get; private set; }

        public DMetadata Metadata => _metadata ?? throw new InvalidOperationException("Нет подключения к серверу Pilot.");

        public DDatabaseInfo DatabaseInfo => _dbInfo ?? throw new InvalidOperationException("Нет подключения к серверу Pilot.");

        public IServerApi ServerApi
        {
            get
            {
                EnsureSession();
                return _serverApi ?? throw new InvalidOperationException("Нет подключения к серверу Pilot.");
            }
        }

        public int CurrentPersonId => DatabaseInfo.Person?.Id ?? 0;

        /// <summary>Корень базы Pilot.</summary>
        public static readonly Guid RootId = new Guid("00000001-0001-0001-0001-000000000001");

        public static PilotServerClient Connect(string serverUrlWithDatabase, string login, string password, int licenseType = 100)
        {
            PilotServerClient client = new PilotServerClient();
            client.Open(serverUrlWithDatabase, login, password, licenseType);
            return client;
        }

        private void Open(string url, string login, string password, int licenseType)
        {
            _url = url;
            _login = login;
            _password = password;
            _licenseType = licenseType;
            ConnectionCredentials credentials = ConnectionCredentials.GetConnectionCredentials(url, login, password.ConvertToSecureString());
            _client = new HttpPilotClient(credentials.GetConnectionString(), credentials.GetConnectionProxy());
            _client.SetConnectionLostListener(new LostListener(this));
            _client.Connect(false);
            IAuthenticationApi auth = _client.GetAuthenticationApi();
            ServerCallback callback = new ServerCallback(OnChangeAsyncError);
            _serverApi = _client.GetServerApi(callback);
            _fileApi = _client.GetFileArchiveApi();
            auth.Login(credentials.DatabaseName, credentials.Username, credentials.ProtectedPassword, false, licenseType);
            _dbInfo = _serverApi.OpenDatabase();
            _metadata = _serverApi.GetMetadata(_dbInfo.MetadataVersion);
            string storageDir = Path.Combine(Path.GetTempPath(), "PilotUITesting", "storage");
            Directory.CreateDirectory(storageDir);
            _storage = new FileSystemStorageProvider(storageDir);
            _backend = CreateBackend();
        }

        /// <summary>
        /// Сервер разорвал сеанс (слушатель HttpPilotClient либо «TransportClient is disposed» при чтении):
        /// подключиться заново с теми же учётными данными. Ошибка переподключения не глушится.
        /// </summary>
        private void EnsureSession()
        {
            string reason = _lostReason;
            if (reason != null)
            {
                Reconnect(reason);
            }
        }

        private void Reconnect(string reason)
        {
            PilotLog.Warn("Сеанс Server API потерян (" + reason + "): переподключение " + (ReconnectCount + 1) + " под " + _login + ".");
            HttpPilotClient old = _client;
            _client = null;
            _serverApi = null;
            _backend = null;
            _fileApi = null;
            try
            {
                (old as IDisposable)?.Dispose();
            }
            catch (Exception ex)
            {
                PilotLog.Warn("Освобождение разорванного подключения завершилось ошибкой: " + ex.Message);
            }
            _lostReason = null;
            Open(_url, _login, _password, _licenseType);
            ReconnectCount++;
        }

        /// <summary>Чтение с одним повтором после разрыва сеанса сервером; запись так не повторяется.</summary>
        private T Read<T>(Func<IServerApi, T> read)
        {
            try
            {
                return read(ServerApi);
            }
            catch (ObjectDisposedException ex) when (ex.Message.IndexOf("TransportClient", StringComparison.Ordinal) >= 0)
            {
                Reconnect(ex.Message);
                return read(ServerApi);
            }
        }

        private sealed class LostListener : IConnectionLostListener
        {
            private readonly PilotServerClient _owner;

            internal LostListener(PilotServerClient owner)
            {
                _owner = owner;
            }

            public void ConnectionLost(Exception ex = null)
            {
                _owner._lostReason = ex?.Message ?? "соединение разорвано сервером";
            }
        }

        public IModifier CreateModifier()
        {
            EnsureSession();
            if (_backend == null)
            {
                throw new InvalidOperationException("Нет подключения к серверу Pilot.");
            }
            return new Modifier(_backend, ChangesetDataSource.Native, MergeChangePolicy.Allow);
        }

        private Backend CreateBackend()
        {
            return new Backend(_serverApi, null, new ChangesetUploader(_fileApi, _storage, null));
        }

        public MType TypeByName(string name)
        {
            MType type = Metadata.Types.FirstOrDefault(t => t.Name == name && !t.IsDeleted);
            if (type == null)
            {
                throw new InvalidOperationException($"Тип '{name}' не найден в базе.");
            }
            return type;
        }

        public bool HasType(string name)
        {
            return Metadata.Types.Any(t => t.Name == name && !t.IsDeleted);
        }

        public MUserState StateByName(string name)
        {
            MUserState state = Metadata.UserStates.FirstOrDefault(s => s.Name == name && !s.IsDeleted);
            if (state == null)
            {
                throw new InvalidOperationException($"Состояние '{name}' не найдено.");
            }
            return state;
        }

        /// <summary>Создать объект с атрибутами; возвращает id. Значения: string, int/long, double, DateTime, decimal, Guid (состояния).</summary>
        public Guid CreateObject(Guid parentId, string typeName, IDictionary<string, object> attributes = null)
        {
            return CreateObjectWithId(Guid.NewGuid(), parentId, typeName, attributes);
        }

        /// <summary>Создать объект с заданным id (например, справочник, на который ссылается конфигурация атрибута).</summary>
        public Guid CreateObjectWithId(Guid id, Guid parentId, string typeName, IDictionary<string, object> attributes = null)
        {
            IModifier modifier = CreateModifier();
            IObjectChangeBuilder builder = modifier.CreateObject(id, parentId, TypeByName(typeName).Id);
            if (attributes != null)
            {
                foreach (KeyValuePair<string, object> pair in attributes)
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }
                    builder.SetAttribute(pair.Key, ToDValue(pair.Value));
                }
            }
            modifier.Apply();
            WaitObject(id);
            return id;
        }

        public void SetAttributes(Guid id, IDictionary<string, object> attributes)
        {
            IModifier modifier = CreateModifier();
            IObjectChangeBuilder builder = modifier.EditObject(id);
            foreach (KeyValuePair<string, object> pair in attributes)
            {
                builder.SetAttribute(pair.Key, ToDValue(pair.Value));
            }
            if (modifier.AnyChanges())
            {
                modifier.Apply();
            }
        }

        public void SetAttribute(Guid id, string name, object value)
        {
            SetAttributes(id, new Dictionary<string, object> { { name, value } });
        }

        /// <summary>Установить состояние (атрибут типа UserState). Сервер проверяет переходы машины состояний.</summary>
        public void SetState(Guid id, string stateAttribute, Guid stateId)
        {
            SetAttribute(id, stateAttribute, stateId);
        }

        public void AddFile(Guid id, string filePath, string snapshotReason = "UI test")
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл не найден", filePath);
            }
            IModifier modifier = CreateModifier();
            modifier.EditObject(id)
                .CreateSnapshot(snapshotReason)
                .AddFile(new DocumentInfo(filePath), _storage);
            if (modifier.AnyChanges())
            {
                modifier.Apply();
            }
        }

        /// <summary>
        /// Содержимое файла с сервера по его телу (DFile.Body): читается частями через файловый архив.
        /// Пустой ответ архива до конца файла - исключение, а не усечённый результат.
        /// </summary>
        public byte[] ReadFileBytes(DFileBody body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }
            EnsureSession();
            if (_fileApi == null)
            {
                throw new InvalidOperationException("Нет подключения к серверу Pilot.");
            }
            const int chunkSize = 1 << 20;
            using (MemoryStream result = new MemoryStream())
            {
                long position = 0;
                while (position < body.Size)
                {
                    int count = (int)Math.Min(chunkSize, body.Size - position);
                    byte[] chunk = _fileApi.GetFileChunk(body.Id, position, count);
                    if (chunk == null || chunk.Length == 0)
                    {
                        throw new IOException($"Файловый архив не вернул данные файла {body.Id} с позиции {position} из {body.Size}.");
                    }
                    result.Write(chunk, 0, chunk.Length);
                    position += chunk.Length;
                }
                return result.ToArray();
            }
        }

        /// <summary>Переместить объект в корзину (аналог удаления из клиента).</summary>
        public void MoveToRecycleBin(Guid id)
        {
            IModifier modifier = CreateModifier();
            modifier.MoveToRecycleBin(id, _storage);
            if (modifier.AnyChanges())
            {
                modifier.Apply();
            }
        }

        public DObject GetObject(Guid id)
        {
            List<DObject> objects = Read(api => api.GetObjects(new[] { id }));
            return objects != null && objects.Count > 0 ? objects[0] : null;
        }

        public List<DObject> GetObjects(IEnumerable<Guid> ids)
        {
            Guid[] array = ids.ToArray();
            return array.Length == 0 ? new List<DObject>() : Read(api => api.GetObjects(array));
        }

        /// <summary>Живые (не в корзине) дочерние объекты.</summary>
        public List<DObject> GetChildren(Guid parentId, string typeName = null)
        {
            DObject parent = GetObject(parentId);
            if (parent == null)
            {
                return new List<DObject>();
            }
            IEnumerable<DChild> children = parent.Children;
            if (typeName != null)
            {
                int typeId = TypeByName(typeName).Id;
                children = children.Where(c => c.TypeId == typeId);
            }
            return GetObjects(children.Select(c => c.ObjectId))
                .Where(o => o.StateInfo == null || o.StateInfo.State == ObjectState.Alive || o.StateInfo.State == ObjectState.Frozen)
                .ToList();
        }

        public DObject FindChild(Guid parentId, string typeName, Func<DObject, bool> predicate = null)
        {
            return GetChildren(parentId, typeName).FirstOrDefault(o => predicate == null || predicate(o));
        }

        /// <summary>Ожидание появления объекта на сервере (изменения применяются асинхронно).</summary>
        public DObject WaitObject(Guid id, int timeoutMs = 15000, Func<DObject, bool> condition = null)
        {
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                DObject obj = GetObject(id);
                if (obj != null && (condition == null || condition(obj)))
                {
                    return obj;
                }
                Thread.Sleep(300);
            }
            throw new TimeoutException($"Объект {id} не появился на сервере за {timeoutMs} мс" + (condition == null ? "" : " или не выполнил условие"));
        }

        /// <summary>Ожидание дочернего объекта заданного типа с условием (например, созданного через UI).</summary>
        public DObject WaitChild(Guid parentId, string typeName, Func<DObject, bool> predicate, int timeoutMs = 20000)
        {
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                DObject found = FindChild(parentId, typeName, predicate);
                if (found != null)
                {
                    return found;
                }
                Thread.Sleep(500);
            }
            throw new TimeoutException($"У объекта {parentId} не появился дочерний объект типа {typeName} за {timeoutMs} мс");
        }

        public static string Attr(DObject obj, string name)
        {
            DValue value;
            if (obj == null || !obj.Attributes.TryGetValue(name, out value) || value == null || value.Value == null)
            {
                return "";
            }
            return value.Value.ToString();
        }

        public static Guid? StateOf(DObject obj, string stateAttribute = "approvedState")
        {
            DValue value;
            if (obj == null || !obj.Attributes.TryGetValue(stateAttribute, out value) || value == null)
            {
                return null;
            }
            return value.GuidValue;
        }

        public string TypeNameOf(DObject obj)
        {
            return Metadata.Types.FirstOrDefault(t => t.Id == obj.TypeId)?.Name ?? obj.TypeId.ToString();
        }

        private static DValue ToDValue(object value)
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
                    return value.ToString();
            }
        }

        public void Dispose()
        {
            try
            {
                _client?.Disconnect();
            }
            catch (Exception ex)
            {
                PilotLog.Warn("Отключение от сервера Pilot завершилось ошибкой: " + ex.Message);
            }
            try
            {
                (_client as IDisposable)?.Dispose();
            }
            catch (Exception ex)
            {
                PilotLog.Warn("Освобождение подключения к серверу Pilot завершилось ошибкой: " + ex.Message);
            }
            _client = null;
            _serverApi = null;
            _backend = null;
        }

        /// <summary>
        /// Уведомления сервера, кроме отказа в асинхронном изменении, тестам не нужны и намеренно не обрабатываются;
        /// отказы (NotifyChangeAsyncError) собираются в <see cref="Errors"/>.
        /// </summary>
        private sealed class ServerCallback : IServerCallback
        {
            private readonly Action<Guid, ProtoExceptionInfo> _onChangeError;

            internal ServerCallback(Action<Guid, ProtoExceptionInfo> onChangeError)
            {
                _onChangeError = onChangeError;
            }

            public void NotifyChangeset(DChangeset changeset)
            {
            }

            public void NotifyOrganisationUnitChangeset(OrganisationUnitChangeset changeset)
            {
            }

            public void NotifyPersonChangeset(PersonChangeset changeset)
            {
            }

            public void NotifyDMetadataChangeset(DMetadataChangeset changeset)
            {
            }

            public void NotifySearchResult(DSearchResult searchResult)
            {
            }

            public void NotifyGeometrySearchResult(DGeometrySearchResult searchResult)
            {
            }

            public void NotifyDNotificationChangeset(DNotificationChangeset changeset)
            {
            }

            public void NotifyCommandResult(Guid requestId, byte[] data, ServerCommandResult result)
            {
            }

            public void NotifyChangeAsyncCompleted(DChangeset changeset)
            {
            }

            public void NotifyChangeAsyncError(Guid identity, ProtoExceptionInfo exception)
            {
                _onChangeError(identity, exception);
            }

            public void NotifyCustomNotification(string name, byte[] data)
            {
            }

            public void NotifyAccessChangeset(Guid objectId)
            {
            }

            public void NotifySettingsChangeset(string settingKey)
            {
            }
        }
    }
}
