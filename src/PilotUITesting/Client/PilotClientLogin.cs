using System;
using PilotTestingTools.Shared;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Сохранённый вход клиента Pilot, по которому <see cref="PilotApp.StartOrAttach"/> входит в базу без окна логина.
    /// Пароль шифруется методом самого клиента (открытый пароль в настройках клиент не принимает). То же делает pilottest --login.
    /// </summary>
    public static class PilotClientLogin
    {
        /// <summary>
        /// Записать вход в настройки клиента из конфигурации (exe, клиент, сервер - serverUrl) под текущим пользователем Windows.
        /// Клиент должен быть закрыт (при выходе он перезаписывает настройки) и хотя бы раз запускаться (файл настроек уже есть).
        /// </summary>
        public static void Save(PilotUiConfig config, string user, string password)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            ClientLoginFile.Save(config.ResolvedExePath, config.ProductFolder, config.ServerUrl, user, password);
            PilotLog.Action($"сохранён вход клиента {config.ProductFolder}: {user} в {config.ServerUrl}");
        }
    }
}
