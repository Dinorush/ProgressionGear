using BepInEx.Unity.IL2CPP;
using ExtraObjectiveSetup.Expedition;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ProgressionGear.Dependencies
{
    internal static class EOSWrapper
    {
        public const string GUID = "Inas.ExtraObjectiveSetup";

        public static bool HasEOS { get; private set; }

        private readonly static HashSet<uint> _targetIDs = new();
        private static bool _allow = false;

        static EOSWrapper()
        {
            HasEOS = IL2CPPChainloader.Instance.Plugins.ContainsKey(GUID);
        }

        public static void CacheLocks()
        {
            if (HasEOS)
                CacheLocksEOS();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CacheLocksEOS()
        {
            var expDef = ExpeditionDefinitionManager.Current.GetDefinition(ExpeditionDefinitionManager.Current.CurrentMainLevelLayout);
            if (expDef == null || expDef.ExpeditionGears == null)
            {
                _allow = false;
                return;
            }

            _allow = expDef.ExpeditionGears.Mode == ExtraObjectiveSetup.Expedition.Gears.Mode.ALLOW;
            expDef.ExpeditionGears.GearIds.ForEach(id => _targetIDs.Add(id));
        }

        public static bool IsGearAllowed(uint id) => HasEOS ? IsGearAllowedEOS(id) : true;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsGearAllowedEOS(uint id)
        {
            return _allow ? _targetIDs.Contains(id) : !_targetIDs.Contains(id);
        }
    }
}
