﻿using CellMenu;
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
}