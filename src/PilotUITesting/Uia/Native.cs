using System;
using System.Runtime.InteropServices;

namespace PilotUITesting.Uia
{
    /// <summary>Win32-проверки окон: закрытость окна определяется по HWND, а не по закэшированному элементу UIA.</summary>
    internal static class Native
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsZoomed(IntPtr hWnd);

        /// <summary>Окно существует и видимо.</summary>
        internal static bool IsShown(IntPtr hWnd)
        {
            return hWnd != IntPtr.Zero && IsWindow(hWnd) && IsWindowVisible(hWnd);
        }

        /// <summary>
        /// Развернуть окно (SW_MAXIMIZE). У главного окна Pilot WindowPattern.SetWindowVisualState(Maximized) не срабатывает
        /// (проверено на Pilot-ICE 26.x). Возвращает, развёрнуто ли окно после вызова.
        /// </summary>
        internal static bool Maximize(IntPtr hWnd)
        {
            ShowWindow(hWnd, 3);
            return IsZoomed(hWnd);
        }
    }
}
