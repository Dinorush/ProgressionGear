using HarmonyLib;
using ProgressionGear.ProgressionLock;
using ProgressionGear.Dependencies;

namespace ProgressionGear.Patches
{
    [HarmonyPatch]
    internal static class RundownManagerPatches
    {
        [HarmonyPatch(typeof(RundownManager), nameof(RundownManager.SetActiveExpedition))]
        [HarmonyAfter(EOSWrapper.INAS_GUID, EOSWrapper.AMOR_GUID)]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_RundownManager_SetActiveExpedition(pActiveExpedition expPackage)
        {
            if (expPackage.tier == eRundownTier.Surface) return;

            GearLockManager.Current.SetupAllowedGearsForActiveRundown();
        }
    }
}
