using Gear;
using HarmonyLib;
using ProgressionWeapons.ProgressionLock;
using ProgressionWeapons.Utils;

namespace ProgressionWeapons.Patches
{
    [HarmonyPatch]
    internal static class GearManagerPatches
    {
        [HarmonyPatch(typeof(GearManager), nameof(GearManager.SetupGearInOfflineMode))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_SetupGearInOfflineMode()
        {
            GearLockManager.Instance.VanillaGearManager = GearManager.Current;

            foreach (var (inventorySlot, loadedGears) in GearLockManager.Instance.GearSlots)
            {
                foreach (GearIDRange gearIDRange in GearManager.Current.m_gearPerSlot[(int)inventorySlot])
                {
                    uint playerOfflineDBPID = gearIDRange.GetOfflineID();
                    loadedGears.TryAdd(playerOfflineDBPID, gearIDRange);
                }
            }
            GearToggleManager.Current.ResetRelatedIDs();
            GearLockManager.Instance.Setup();
        }
    }
}
