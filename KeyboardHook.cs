using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace PeekThrough
{
    internal class KeyboardHook : IDisposable
    {
        private NativeMethods.LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private SynchronizationContext _syncContext;

        public event Action OnLWinDown;
        public event Action OnLWinUp;

        public KeyboardHook()
        {
            _proc = HookCallback;
            _syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            _hookID = SetHook(_proc);
        }

        public void Dispose()
        {
            NativeMethods.UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, proc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == NativeMethods.VK_LWIN)
                {
                    if (wParam == (IntPtr)NativeMethods.WM_KEYDOWN)
                    {
                        _syncContext.Post(state =>
                        {
                            Action handler = OnLWinDown;
                            if (handler != null) handler();
                        }, null);
                        // Swallow the key to prevent Start Menu from popping up immediately
                        // Application logic will simulate it later if needed.
                        return (IntPtr)1; 
                    }
                    else if (wParam == (IntPtr)NativeMethods.WM_KEYUP)
                    {
                        _syncContext.Post(state =>
                        {
                            Action handler = OnLWinUp;
                            if (handler != null) handler();
                        }, null);
                        return (IntPtr)1;
                    }
                }
            }
            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
