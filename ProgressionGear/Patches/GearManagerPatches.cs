using Gear;
using HarmonyLib;
using ProgressionGear.ProgressionLock;
using ProgressionGear.Utils;

namespace ProgressionGear.Patches
{
    [HarmonyPatch]
    internal static class GearManagerPatches
    {
        [HarmonyPatch(typeof(GearManager), nameof(GearManager.SetupGearInOfflineMode))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_SetupGearInOfflineMode()
        {
            GearLockManager.Current.VanillaGearManager = GearManager.Current;

            foreach (var (inventorySlot, loadedGears) in GearLockManager.Current.GearSlots)
            {
                foreach (GearIDRange gearIDRange in GearManager.Current.m_gearPerSlot[(int)inventorySlot])
                {
                    uint playerOfflineDBPID = gearIDRange.GetOfflineID();
                    loadedGears.TryAdd(playerOfflineDBPID, gearIDRange);
                }
            }
            GearToggleManager.Current.ResetRelatedIDs();
        }
    }
}
