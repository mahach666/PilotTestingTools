using System;
using System.Runtime.InteropServices;
using FlaUI.Core.Exceptions;

namespace PilotUITesting.Uia
{
    /// <summary>Вид сбоя запроса UI Automation.</summary>
    internal enum UiFaultKind
    {
        /// <summary>Не сбой UI Automation: ошибка в коде теста или непредвиденный отказ - пробрасывается.</summary>
        Other,

        /// <summary>Поток UI клиента не ответил вовремя (UIA_E_TIMEOUT / RPC_E_TIMEOUT): элемент может быть, проверить не удалось.</summary>
        Busy,

        /// <summary>Элемент исчез из дерева (UIA_E_ELEMENTNOTAVAILABLE): окно закрылось, строка удалена.</summary>
        Gone,
    }

    /// <summary>
    /// Классификация исключений UI Automation по коду HRESULT.
    /// FlaUI (Tools.Com.ConvertException) превращает UIA_E_TIMEOUT в TimeoutException("UIA Timeout") с вложенным COMException,
    /// UIA_E_ELEMENTNOTAVAILABLE - в ElementNotAvailableException; прочие коды COM он переводит в Win32Exception и код теряет -
    /// такие сбои считаются «прочими» и пробрасываются, а не угадываются.
    /// </summary>
    internal static class UiFault
    {
        internal const int UiaTimeout = unchecked((int)0x80131505);
        internal const int RpcTimeout = unchecked((int)0x8001011F);
        internal const int ElementNotAvailable = unchecked((int)0x80040201);

        internal static UiFaultKind Classify(Exception ex)
        {
            for (Exception current = ex; current != null; current = current.InnerException)
            {
                if (current is ClientBusyException)
                {
                    // вложенное чтение не дождалось ответа клиента - для внешнего ожидания это та же занятость
                    return UiFaultKind.Busy;
                }
                if (current is PilotWaitTimeoutException)
                {
                    // собственный таймаут ожидания - не занятость клиента
                    return UiFaultKind.Other;
                }
                if (current is ElementNotAvailableException)
                {
                    return UiFaultKind.Gone;
                }
                if (current is COMException com)
                {
                    if (com.ErrorCode == UiaTimeout || com.ErrorCode == RpcTimeout)
                    {
                        return UiFaultKind.Busy;
                    }
                    if (com.ErrorCode == ElementNotAvailable)
                    {
                        return UiFaultKind.Gone;
                    }
                    return UiFaultKind.Other;
                }
            }
            return UiFaultKind.Other;
        }

        internal static bool IsTransient(Exception ex)
        {
            return Classify(ex) != UiFaultKind.Other;
        }
    }
}
