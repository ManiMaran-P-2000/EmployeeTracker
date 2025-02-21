using EmployeeTracker1.MAUI.Services.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracker1.MAUI.Platforms.Windows.Services
{
    public class WindowsRestrictionService : IWindowRestrictionService
    {
        private readonly ILogger<WindowsRestrictionService> _logger;
        private IntPtr _hookID = IntPtr.Zero;
        private WNDPROC _newWndProc;
        private IntPtr _originalWndProcPtr;

        public WindowsRestrictionService(ILogger<WindowsRestrictionService> logger)
        {
            _logger = logger;
        }

        public void RestrictWindow()
        {
            var window = Application.Current.Windows.FirstOrDefault();
            if (window != null)
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window.Handler.PlatformView);
                ForceTopMostAndExclusive(hWnd);
                //DisableTaskSwitching();
                PreventClosing(hWnd);
            }
        }

        public void RestoreWindow()
        {
            var window = Application.Current.Windows.FirstOrDefault();
            if (window != null)
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window.Handler.PlatformView);
                RestoreWindowBehavior(hWnd);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern long SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll")]
        private static extern long GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DrawMenuBar(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr WNDPROC(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int GWL_STYLE = -16;
        private const long WS_MINIMIZEBOX = 0x00020000;
        private const long WS_MAXIMIZEBOX = 0x00010000;
        private const long WS_THICKFRAME = 0x00040000;
        private const long WS_SYSMENU = 0x00080000;
        private const int SW_SHOWNORMAL = 1;
        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint SC_CLOSE = 0xF060;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int GWL_WNDPROC = -4;
        private const int WH_KEYBOARD_LL = 13;

        private void ForceTopMostAndExclusive(IntPtr hWnd)
        {
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 900, 800, SWP_SHOWWINDOW);
            long style = GetWindowLong(hWnd, GWL_STYLE);
            style &= ~(WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_THICKFRAME | WS_SYSMENU);
            SetWindowLong(hWnd, GWL_STYLE, style);
        }

        private void DisableTaskSwitching()
        {
            IntPtr hInstance = GetModuleHandle(null);
            _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, HookCallback, hInstance, 0);
        }

        private int HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isAltPressed = (GetAsyncKeyState(0x12) & 0x8000) != 0;
                if ((isAltPressed && vkCode == 0x09) || // Alt+Tab
                    (isAltPressed && vkCode == 0x1B) || // Alt+Esc
                    vkCode == 0x5B || vkCode == 0x5C)   // Win keys
                {
                    return 1;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void PreventClosing(IntPtr hWnd)
        {
            IntPtr hMenu = GetSystemMenu(hWnd, false);
            if (hMenu != IntPtr.Zero)
            {
                RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
                DrawMenuBar(hWnd);
            }

            _originalWndProcPtr = GetWindowLongPtr(hWnd, GWL_WNDPROC);
            _newWndProc = (hWnd, msg, wParam, lParam) =>
            {
                if (msg == WM_SYSCOMMAND && (wParam.ToInt32() & 0xFFF0) == SC_CLOSE)
                    return IntPtr.Zero;
                return CallWindowProc(_originalWndProcPtr, hWnd, msg, wParam, lParam);
            };
            SetWindowLongPtr(hWnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
        }

        private void RestoreWindowBehavior(IntPtr hWnd)
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }

            if (_originalWndProcPtr != IntPtr.Zero && _newWndProc != null)
            {
                SetWindowLongPtr(hWnd, GWL_WNDPROC, _originalWndProcPtr);
                _newWndProc = null;
            }

            long style = GetWindowLong(hWnd, GWL_STYLE);
            style |= WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_THICKFRAME | WS_SYSMENU;
            SetWindowLong(hWnd, GWL_STYLE, style);

            SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_SHOWWINDOW | 0x0002 | 0x0001);

            IntPtr hMenu = GetSystemMenu(hWnd, true);
            DrawMenuBar(hWnd);
        }
    }
}
