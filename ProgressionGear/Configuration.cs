using BepInEx;
using BepInEx.Configuration;
using GTFO.API.Utilities;
using ProgressionGear.ProgressionLock;
using System;
using System.IO;

namespace ProgressionGear
{
    internal static class Configuration
    {
        private readonly static ConfigEntry<bool> _disableProgression;
        public static bool DisableProgression => _disableProgression.Value;
        private readonly static ConfigEntry<bool> _toggleBlink;
        public static bool ToggleBlink => _toggleBlink.Value;

        private static readonly ConfigFile _configFile;
        private static bool _disabled = false;

        static Configuration()
        {
            _configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, EntryPoint.MODNAME + ".cfg"), saveOnInit: true);
            string section = "Override";
            _disableProgression = _configFile.Bind(section, "Disable Progression Locks", false, "Disables progression-locking for weapons.");
            _disabled = _disableProgression.Value;

            section = "General Settings";
            _toggleBlink = _configFile.Bind(section, "Toggle Blink", true, "Enables the blinking effect on gear toggle buttons.");
        }

        public static void Init()
        {
            SafeFileSystemWatcher.Create(_configFile);
            _configFile.ConfigReloaded += OnFileChanged;
        }

        private static void OnFileChanged(object? sender, EventArgs e)
        {
            if (_disabled != DisableProgression)
            {
                _disabled = !_disabled;
                GearLockManager.Current.SetupAllowedGearsForActiveRundown();
            }
        }
    }
}
