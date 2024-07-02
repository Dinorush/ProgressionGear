using CellMenu;
using HarmonyLib;
using ProgressionGear.Dependencies;
using ProgressionGear.ProgressionLock;

namespace ProgressionGear.Patches
{
    [HarmonyPatch]
    internal static class PageRundownPatches
    {
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.PlaceRundown))]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
        [HarmonyAfter(LocalProgressionWrapper.GUID)]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_RundownUpdate()
        {
            GearLockManager.Instance.SetupAllowedGearsForActiveRundown();
        }
    }

    internal static class PageRundownPatches_NoLP
    {
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.SetIconStatus))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_IconUpdate(CM_ExpeditionIcon_New icon, string mainFinishCount, string secondFinishCount, string thirdFinishCount, string allFinishedCount)
        {
            ProgressionData data = new();
            if (mainFinishCount != "-") data.MainCompletionCount = int.Parse(mainFinishCount);
            if (secondFinishCount != "-") data.SecondaryCompletionCount = int.Parse(secondFinishCount);
            if (thirdFinishCount != "-") data.ThirdCompletionCount = int.Parse(thirdFinishCount);
            if (allFinishedCount != "-") data.AllCompletionCount = int.Parse(allFinishedCount);

            ProgressionWrapper.UpdateNativeProgression(icon.Tier, icon.ExpIndex, data);
        }
    }
}
