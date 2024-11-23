using CellMenu;
using HarmonyLib;
using ProgressionGear.ProgressionLock;

namespace ProgressionGear.Patches
{
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
