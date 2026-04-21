using System;
using System.Linq;
using GhostThrough.Models;

namespace GhostThrough
{
    internal sealed class AppContext : IDisposable
    {
        public Settings Settings { get; private set; }
        public SettingsManager SettingsManager { get; private set; }
        public GhostController Controller { get; private set; }
        public KeyboardHook KeyboardHook { get; private set; }
        public MouseHook MouseHook { get; private set; }
        public ProfileManager ProfileManager { get; private set; }

        private bool _shutdown;

        private AppContext()
        {
        }

        public static AppContext Create(string settingsPath)
        {
            var settingsManager = new SettingsManager(settingsPath);
            var settings = settingsManager.LoadSettings();
            var profileManager = new ProfileManager(
                settings.Profiles.ActiveId,
                settings.Profiles.List.Select(p => new Profile(p.Id, p.Name, p.Opacity)).ToList());

            var activationType = settings.Activation.Type.ToActivationInputType();

            var controller = new GhostController(activationType, profileManager);
            controller.ActivationKeyCode = GhostController.NormalizeActivationKeyCode(settings.Activation.KeyCode);

            var appContext = new AppContext
            {
                Settings = settings,
                SettingsManager = settingsManager,
                ProfileManager = profileManager,
                Controller = controller,
                KeyboardHook = new KeyboardHook(controller),
                MouseHook = new MouseHook(controller, settings.Activation.MouseButton)
            };

            appContext.WireEvents();
            return appContext;
        }

        private void WireEvents()
        {
            if (ProfileManager != null)
                ProfileManager.OnProfileChanged += OnProfileChanged;
        }

        private void UnwireEvents()
        {
            if (ProfileManager != null)
                ProfileManager.OnProfileChanged -= OnProfileChanged;
        }

        private void OnProfileChanged(Profile profile)
        {
            if (_shutdown)
                return;

            Save();
        }

        public void Reconfigure(ActivationInputType activationType, int activationKeyCode, int mouseButton)
        {
            if (Controller.IsGhostModeActive)
                Controller.DeactivateGhostMode();

            Controller.OnOtherInputBeforeActivation();

            activationKeyCode = GhostController.NormalizeActivationKeyCode(activationKeyCode);

            Controller.CurrentActivationType = activationType;
            Controller.ActivationKeyCode = activationKeyCode;
            MouseHook.SetSelectedMouseButton(mouseButton);

            Settings.Activation.Type = activationType.ToSettingsValue();
            Settings.Activation.KeyCode = activationKeyCode;
            Settings.Activation.MouseButton = mouseButton;

            Save();
        }

        public void Save()
        {
            Settings.Profiles.ActiveId = ProfileManager.ActiveProfile.Id;
            SettingsManager.SaveSettings(Settings);
        }

        public void Shutdown()
        {
            if (_shutdown)
                return;

            _shutdown = true;
            UnwireEvents();
            Save();

            if (MouseHook != null)
                MouseHook.Dispose();
            if (KeyboardHook != null)
                KeyboardHook.Dispose();
            if (Controller != null)
                Controller.Dispose();

            DebugLogger.Flush();
        }

        public void Dispose()
        {
            Shutdown();
        }
    }
}
