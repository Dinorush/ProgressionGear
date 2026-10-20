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

        public static void CacheLocks(ref HashSet<uint>? whitelist, HashSet<uint> blacklist)
        {
            if (HasEOS)
            {
                if (_isAmor)
                    CacheLocksEOS_Amor(ref whitelist, blacklist);
                else
                    CacheLocksEOS_Inas(ref whitelist, blacklist);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CacheLocksEOS_Inas(ref HashSet<uint>? whitelist, HashSet<uint> blacklist)
        {
            var expDef = InasExpedition.ExpeditionDefinitionManager.Current.GetDefinition(InasExpedition.ExpeditionDefinitionManager.Current.CurrentMainLevelLayout);
            if (expDef == null || expDef.ExpeditionGears == null) return;

            if (expDef.ExpeditionGears.Mode == InasExpedition.Gears.Mode.ALLOW)
            {
                whitelist ??= new();
                whitelist.EnsureCapacity(expDef.ExpeditionGears.GearIds.Count + whitelist.Count);
                whitelist.UnionWith(expDef.ExpeditionGears.GearIds);
            }
            else
            {
                blacklist.EnsureCapacity(expDef.ExpeditionGears.GearIds.Count + blacklist.Count);
                blacklist.UnionWith(expDef.ExpeditionGears.GearIds);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CacheLocksEOS_Amor(ref HashSet<uint>? whitelist, HashSet<uint> blacklist)
        {
            if (!AmorExpedition.ExpeditionDefinitionManager.Current.TryGetDefinition(AmorExpedition.ExpeditionDefinitionManager.CurrentMainLevelLayout, out var expDef) || expDef.ExpeditionGears == null) return;

            if (expDef.ExpeditionGears.Mode == AmorExpedition.Gears.Mode.ALLOW)
            {
                whitelist ??= new();
                whitelist.EnsureCapacity(expDef.ExpeditionGears.GearIds.Count + whitelist.Count);
                whitelist.UnionWith(expDef.ExpeditionGears.GearIds);
            }
            else
            {
                blacklist.EnsureCapacity(expDef.ExpeditionGears.GearIds.Count + blacklist.Count);
                blacklist.UnionWith(expDef.ExpeditionGears.GearIds);
            }
        }
    }
}
