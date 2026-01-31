using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace PeekThrough
{
    internal class GhostLogic : IDisposable
    {
        private readonly object _lockObject = new object();
        private Timer _timer;
        private bool _isLWinDown;
        private bool _ghostModeActive;
        private IntPtr _targetHwnd = IntPtr.Zero;
        private int _originalExStyle;
        private bool _hasOriginalExStyle = false;

        private Form _tooltipForm;
        private Label _tooltipLabel;

        public GhostLogic()
        {
            _timer = new Timer();
            _timer.Interval = 500; // 0.5 seconds
            _timer.Tick += OnTimerTick;

            // Initialize Tooltip Form
            _tooltipForm = new Form();
            _tooltipForm.FormBorderStyle = FormBorderStyle.None;
            _tooltipForm.ShowInTaskbar = false;
            _tooltipForm.TopMost = true;
            _tooltipForm.BackColor = Color.LightYellow;
            _tooltipForm.Size = new Size(100, 30);
            _tooltipForm.StartPosition = FormStartPosition.Manual;
            _tooltipForm.Opacity = 0.9;
            
            _tooltipLabel = new Label();
            _tooltipLabel.Text = "👻 Ghost Mode";
            _tooltipLabel.AutoSize = true;
            _tooltipLabel.Location = new Point(5, 5);
            _tooltipForm.Controls.Add(_tooltipLabel);
            _tooltipForm.AutoSize = true;
            _tooltipLabel.AutoSize = true;
        }

        public void OnKeyDown()
        {
            lock (_lockObject)
            {
                if (_isLWinDown) return;
                _isLWinDown = true;
                _timer.Start();
            }
        }

        public void OnKeyUp()
        {
            lock (_lockObject)
            {
                _isLWinDown = false;
                _timer.Stop();

                if (_ghostModeActive)
                {
                    // Deactivate Ghost Mode
                    RestoreWindow();
                    HideTooltip();
                    NativeMethods.Beep(500, 50);
                    _ghostModeActive = false;
                }
                else
                {
                    // It was a short press, trigger Start Menu
                    SendLWinClick();
                }
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            lock (_lockObject)
            {
                _timer.Stop(); // One-shot trigger check
                if (_isLWinDown)
                {
                    ActivateGhostMode();
                }
            }
        }

        private void ActivateGhostMode()
        {
            Point cursorPos;
            NativeMethods.GetCursorPos(out cursorPos);
            IntPtr hwnd = NativeMethods.WindowFromPoint(cursorPos);
            // Get the root window (ancestor) because we might be hovering a child control
            hwnd = NativeMethods.GetAncestor(hwnd, NativeMethods.GA_ROOT);

            // Check if valid window and not desktop/taskbar
            StringBuilder className = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, className, 256);
            string cls = className.ToString();

            lock (_lockObject)
            {
                if (cls == "Progman" || cls == "WorkerW" || cls == "Shell_TrayWnd")
                {
                    // Ignore these windows, just act as if held but do nothing
                    // The AHK script just waits for key up in this case without doing anything.
                     // We simply mark ghost mode active so we don't trigger Start Menu, but we don't apply effects.
                     _ghostModeActive = true; 
                     // Optionally continue waiting
                     return;
                }

                _targetHwnd = hwnd;
                _ghostModeActive = true;
            }

            try
            {
                // Apply Transparency
                _originalExStyle = NativeMethods.GetWindowLong(_targetHwnd, NativeMethods.GWL_EXSTYLE);
                _hasOriginalExStyle = true;

                int newStyle = _originalExStyle | NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT;
                NativeMethods.SetWindowLong(_targetHwnd, NativeMethods.GWL_EXSTYLE, newStyle);
                NativeMethods.SetLayeredWindowAttributes(_targetHwnd, 0, 80, NativeMethods.LWA_ALPHA); // 80/255 opacity (~30%)

                // Show Tooltip
                ShowTooltip(cursorPos);
                NativeMethods.Beep(1000, 50);
            }
            catch
            {
                // Fail silently or log
                RestoreWindow();
            }
        }

        private void RestoreWindow()
        {
            if (_targetHwnd != IntPtr.Zero && _hasOriginalExStyle)
            {
                // Проверка валидности окна перед манипуляциями
                if (!NativeMethods.IsWindow(_targetHwnd))
                {
                    // Окно уже закрыто, просто сбрасываем состояние
                    _targetHwnd = IntPtr.Zero;
                    _hasOriginalExStyle = false;
                    return;
                }

                try
                {
                    NativeMethods.SetWindowLong(_targetHwnd, NativeMethods.GWL_EXSTYLE, _originalExStyle);
                    
                    // Восстановление прозрачности
                    if ((_originalExStyle & NativeMethods.WS_EX_LAYERED) != 0)
                    {
                        NativeMethods.SetLayeredWindowAttributes(_targetHwnd, 0, 255, NativeMethods.LWA_ALPHA);
                    }
                }
                catch (Exception ex)
                {
                    // Логирование ошибки в Debug output
                    System.Diagnostics.Debug.WriteLine("RestoreWindow error: " + ex.Message);
                }
                finally
                {
                    _targetHwnd = IntPtr.Zero;
                    _hasOriginalExStyle = false;
                }
            }
        }

        private void ShowTooltip(Point location)
        {
            _tooltipForm.Location = new Point(location.X + 20, location.Y + 20);
            _tooltipForm.Show();
        }

        private void HideTooltip()
        {
             _tooltipForm.Hide();
        }

        private void SendLWinClick()
        {
            // Simulate LWin Down and Up
            NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[2];

            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].U.ki.wVk = NativeMethods.VK_LWIN;
            inputs[0].U.ki.dwFlags = 0; // KeyDown

            inputs[1].type = NativeMethods.INPUT_KEYBOARD;
            inputs[1].U.ki.wVk = NativeMethods.VK_LWIN;
            inputs[1].U.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;

            NativeMethods.SendInput(2, inputs, NativeMethods.INPUT.Size);
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                if (_timer != null) _timer.Dispose();
                if (_tooltipForm != null) _tooltipForm.Dispose();
                RestoreWindow();
            }
        }
    }
}
