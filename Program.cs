using System;
using System.Windows.Forms;

namespace PeekThrough
{
    internal static class Program
    {
        private static KeyboardHook _keyboardHook;
        private static MouseHook _mouseHook;
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

                // Создаем GhostLogic с типом активации по умолчанию (клавиатура)
                _logic = new GhostLogic(GhostLogic.ActivationType.Keyboard);
                
                // Инициализируем хуки
                _keyboardHook = new KeyboardHook(_logic);
                _mouseHook = new MouseHook(_logic, NativeMethods.VK_MBUTTON); // По умолчанию используем среднюю кнопку мыши

                // Подписываемся на события
                SubscribeHookEvents();

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
                    // Добавляем пункт для настройки активации
                    contextMenu.MenuItems.Add("Change Activation Method", (s, e) => ShowActivationSettings());
                    contextMenu.MenuItems.Add("-");
                    contextMenu.MenuItems.Add("Exit", (s, e) => Application.Exit());
                    trayIcon.ContextMenu = contextMenu;
                    trayIcon.Visible = true;

                    // Create a dummy ApplicationContext to run the loop without a main form visible at start
                    Application.Run();

                    trayIcon.Visible = false;
                }

                _keyboardHook.Dispose();
                _mouseHook.Dispose();
                _logic.Dispose();
            }
        }
        
        // Подписка событий хуков на текущий _logic
        private static void SubscribeHookEvents()
        {
            // Подписываемся на события клавиатуры
            _keyboardHook.OnLWinDown += _logic.OnKeyDown;
            _keyboardHook.OnLWinUp += _logic.OnKeyUp;
            _keyboardHook.OnOtherKeyPressedBeforeWin += _logic.BlockGhostMode;

            // Подписываемся на события мыши
            _mouseHook.OnSelectedMouseDown += _logic.OnMouseButtonDown;
            _mouseHook.OnSelectedMouseUp += _logic.OnMouseButtonUp;
            _mouseHook.OnOtherMouseButtonPressedBeforeSelected += _logic.BlockGhostMode;
        }
        
        // Отписка событий хуков от текущего _logic
        private static void UnsubscribeHookEvents()
        {
            _keyboardHook.OnLWinDown -= _logic.OnKeyDown;
            _keyboardHook.OnLWinUp -= _logic.OnKeyUp;
            _keyboardHook.OnOtherKeyPressedBeforeWin -= _logic.BlockGhostMode;

            _mouseHook.OnSelectedMouseDown -= _logic.OnMouseButtonDown;
            _mouseHook.OnSelectedMouseUp -= _logic.OnMouseButtonUp;
            _mouseHook.OnOtherMouseButtonPressedBeforeSelected -= _logic.BlockGhostMode;
        }
        
        // Безопасное переключение метода активации
        private static void SwitchActivation(GhostLogic.ActivationType type, int mouseButton = NativeMethods.VK_MBUTTON)
        {
            // Деактивируем Ghost Mode если активен
            if (_logic.IsGhostModeActive)
                _logic.DeactivateGhostMode();
            
            // Отписываемся от старого logic
            UnsubscribeHookEvents();
            
            // Dispose старого logic
            _logic.Dispose();
            
            // Создаём новый logic с нужным типом активации
            _logic = new GhostLogic(type);
            
            // Переподписываем оба хука на новый logic
            SubscribeHookEvents();
            
            // Устанавливаем кнопку мыши если нужно
            if (type == GhostLogic.ActivationType.Mouse)
                _mouseHook.SetSelectedMouseButton(mouseButton);
        }
        
        // Метод для показа настроек активации
        private static void ShowActivationSettings()
        {
            var menu = new ContextMenuStrip();
            
            // Добавляем пункты для выбора типа активации
            var keyboardItem = new ToolStripMenuItem("Keyboard (Win Key)");
            var mouseMiddleItem = new ToolStripMenuItem("Mouse (Middle Button)");
            var mouseRightItem = new ToolStripMenuItem("Mouse (Right Button)");
            var mouseX1Item = new ToolStripMenuItem("Mouse (X1 Button)");
            var mouseX2Item = new ToolStripMenuItem("Mouse (X2 Button)");
            
            // Устанавливаем галочки для текущего типа активации
            switch (_logic.CurrentActivationType)
            {
                case GhostLogic.ActivationType.Keyboard:
                    keyboardItem.Checked = true;
                    break;
                case GhostLogic.ActivationType.Mouse:
                    // Проверяем, какая кнопка мыши используется
                    int selectedMouseButton = _mouseHook.SelectedMouseButton;
                    if (selectedMouseButton == NativeMethods.VK_MBUTTON)
                        mouseMiddleItem.Checked = true;
                    else if (selectedMouseButton == NativeMethods.VK_RBUTTON)
                        mouseRightItem.Checked = true;
                    else if (selectedMouseButton == NativeMethods.VK_XBUTTON1)
                        mouseX1Item.Checked = true;
                    else if (selectedMouseButton == NativeMethods.VK_XBUTTON2)
                        mouseX2Item.Checked = true;
                    break;
            }
            
            // Обработчики для изменения типа активации
            keyboardItem.Click += (s, e) => SwitchActivation(GhostLogic.ActivationType.Keyboard);
            mouseMiddleItem.Click += (s, e) => SwitchActivation(GhostLogic.ActivationType.Mouse, NativeMethods.VK_MBUTTON);
            mouseRightItem.Click += (s, e) => SwitchActivation(GhostLogic.ActivationType.Mouse, NativeMethods.VK_RBUTTON);
            mouseX1Item.Click += (s, e) => SwitchActivation(GhostLogic.ActivationType.Mouse, NativeMethods.VK_XBUTTON1);
            mouseX2Item.Click += (s, e) => SwitchActivation(GhostLogic.ActivationType.Mouse, NativeMethods.VK_XBUTTON2);
            
            menu.Items.Add(keyboardItem);
            menu.Items.Add(mouseMiddleItem);
            menu.Items.Add(mouseRightItem);
            menu.Items.Add(mouseX1Item);
            menu.Items.Add(mouseX2Item);
            
            // Закрываем меню после выбора пункта
            menu.ItemClicked += (s, e) => menu.Close();
            
            // Показываем меню в позиции курсора
            menu.Show(System.Windows.Forms.Cursor.Position);
        }
    }
}
