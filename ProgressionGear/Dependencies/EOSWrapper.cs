using BepInEx.Unity.IL2CPP;
using InasExpedition = ExtraObjectiveSetup.Expedition;
using AmorExpedition = EOS.Modules.Expedition;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ProgressionGear.Dependencies
{
    internal static class EOSWrapper
    {
        public const string INAS_GUID = "Inas.ExtraObjectiveSetup";
        public const string AMOR_GUID = "Amor.ExcellentObjectiveSetup";

        public static bool HasEOS { get; private set; }
        private readonly static bool _isAmor = false;

        private readonly static HashSet<uint> _targetIDs = new();
        private static bool _allow = false;

        static EOSWrapper()
        {
            if (IL2CPPChainloader.Instance.Plugins.ContainsKey(INAS_GUID))
            {
                HasEOS = true;
            }
            else if (IL2CPPChainloader.Instance.Plugins.ContainsKey(AMOR_GUID))
            {
                HasEOS = true;
                _isAmor = true;
            }
            else
            {
                HasEOS = false;
            }
        }

        public static void CacheLocks()
        {
            if (HasEOS)
            {
                if (_isAmor)
                    CacheLocksEOS_Amor();
                else
                    CacheLocksEOS_Inas();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CacheLocksEOS_Inas()
        {
            _targetIDs.Clear();
            var expDef = InasExpedition.ExpeditionDefinitionManager.Current.GetDefinition(InasExpedition.ExpeditionDefinitionManager.Current.CurrentMainLevelLayout);
            if (expDef == null || expDef.ExpeditionGears == null)
            {
                _allow = false;
                return;
            }

            _allow = expDef.ExpeditionGears.Mode == InasExpedition.Gears.Mode.ALLOW;
            expDef.ExpeditionGears.GearIds.ForEach(id => _targetIDs.Add(id));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CacheLocksEOS_Amor()
        {
            _targetIDs.Clear();
            if (!AmorExpedition.ExpeditionDefinitionManager.Current.TryGetDefinition(AmorExpedition.ExpeditionDefinitionManager.CurrentMainLevelLayout, out var expDef) || expDef.ExpeditionGears == null)
            {
                _allow = false;
                return;
            }

            _allow = expDef.ExpeditionGears.Mode == AmorExpedition.Gears.Mode.ALLOW;
            expDef.ExpeditionGears.GearIds.ForEach(id => _targetIDs.Add(id));
        }

        public static bool IsGearAllowed(uint id) => !HasEOS || IsGearAllowedEOS(id);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsGearAllowedEOS(uint id)
        {
            return _allow ? _targetIDs.Contains(id) : !_targetIDs.Contains(id);
        }
    }
}
