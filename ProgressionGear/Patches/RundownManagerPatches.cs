using GameData;
using HarmonyLib;
using ProgressionGear.ProgressionLock;
using ProgressionGear.Dependencies;

namespace ProgressionGear.Patches
{
    internal static class RundownManagerPatches_EOS
    {
        [HarmonyPatch(typeof(RundownManager), nameof(RundownManager.SetActiveExpedition))]
        [HarmonyAfter(EOSWrapper.GUID)]
        [HarmonyPostfix]
        private static void Post_RundownManager_SetActiveExpedition(pActiveExpedition expPackage)
        {
            if (expPackage.tier == eRundownTier.Surface) return;

            GearLockManager.Current.LockGearForActiveRundown();
        }
    }
}
