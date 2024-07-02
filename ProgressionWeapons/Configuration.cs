using BepInEx.Configuration;
using BepInEx;
using System.IO;
using GTFO.API.Utilities;

namespace ProgressionWeapons
{
    internal static class Configuration
    {
        public static bool DisableProgression { get; set; } = false;

        private static readonly ConfigFile _configFile;

        static Configuration()
        {
            _configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, EntryPoint.MODNAME + ".cfg"), saveOnInit: true);
            BindAll(_configFile);
        }

        public static void Init()
        {
            LiveEdit.CreateListener(Paths.ConfigPath, EntryPoint.MODNAME + ".cfg", false).FileChanged += OnFileChanged;
        }

        private static void OnFileChanged(LiveEditEventArgs _)
        {
            _configFile.Reload();
            DisableProgression = (bool)_configFile["Override", "Disable Progression Locks"].BoxedValue;
        }

        private static void BindAll(ConfigFile config)
        {
            DisableProgression = config.Bind("Override", "Disable Progression Locks", DisableProgression, "Disables progression-locking for weapons.").Value;
        }
    }
}
