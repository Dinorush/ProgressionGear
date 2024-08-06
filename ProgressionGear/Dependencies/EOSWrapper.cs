using BepInEx.Unity.IL2CPP;
using ExtraObjectiveSetup.Expedition.Gears;
using Gear;

namespace ProgressionGear.Dependencies
{
    internal static class EOSWrapper
    {
        public const string GUID = "Inas.ExtraObjectiveSetup";

        public static bool HasEOS { get; private set; }

        static EOSWrapper()
        {
            HasEOS = IL2CPPChainloader.Instance.Plugins.ContainsKey(GUID);
        }
    }
}
