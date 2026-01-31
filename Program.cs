using System;
using System.Windows.Forms;

namespace PeekThrough
{
    internal static class Program
    {
        private static KeyboardHook _hook;
        private static GhostLogic _logic;

        [STAThread]
        static void Main()
        {
            // Ensure single instance
            using (var mutex = new System.Threading.Mutex(false, "PeekThroughGhostModeApp"))
            {
                if (!mutex.WaitOne(0, false))
                {
                    // Already running
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                _logic = new GhostLogic();
                _hook = new KeyboardHook(_logic);

                _hook.OnLWinDown += _logic.OnKeyDown;
                _hook.OnLWinUp += _logic.OnKeyUp;
                _hook.OnOtherKeyPressedBeforeWin += _logic.BlockGhostMode;

                // Setup Tray Icon
                using (var trayIcon = new NotifyIcon())
                {
                    trayIcon.Text = "PeekThrough Ghost Mode";
                    
                    // Try to load icon from resources folder
                    string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "icons", "icon.ico");
                    if (System.IO.File.Exists(iconPath))
                    {
                        trayIcon.Icon = new System.Drawing.Icon(iconPath);
                    }
                    else
                    {
                        // Fallback to generic icon if file not found
                        trayIcon.Icon = System.Drawing.SystemIcons.Application;
                    }

                    var contextMenu = new ContextMenu();
                    contextMenu.MenuItems.Add("Exit", (s, e) => Application.Exit());
                    trayIcon.ContextMenu = contextMenu;
                    trayIcon.Visible = true;

                    // Create a dummy ApplicationContext to run the loop without a main form visible at start
                    Application.Run();

                    trayIcon.Visible = false;
                }

                _hook.Dispose();
                _logic.Dispose();
            }
        }
    }
}
