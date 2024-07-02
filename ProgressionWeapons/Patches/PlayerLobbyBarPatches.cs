using CellMenu;
using Gear;
using HarmonyLib;
using Player;
using ProgressionWeapons.ProgressionLock;
using ProgressionWeapons.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgressionWeapons.Patches
{
    [HarmonyPatch]
    internal static class PlayerLobbyBarPatches
    {
        public static GameObject? SwapButton;

        private static CM_InventorySlotItem? _cachedItem;
        [HarmonyPatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.UpdateWeaponWindowInfo))]
        [HarmonyPrefix]
        private static void Pre_LoadoutHeaderSelected(CM_PlayerLobbyBar __instance)
        {
            _cachedItem = __instance.selectedWeaponSlotItem;
        }

        [HarmonyPatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.UpdateWeaponWindowInfo))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_LoadoutHeaderSelected(CM_PlayerLobbyBar __instance, InventorySlot slot)
        {
            // If the player's selected weapon was already found, don't need to do anything.
            if (_cachedItem != __instance.selectedWeaponSlotItem) return;
            // Otherwise, they may have selected a weapon that isn't the default slot of its toggle list.
            // Need to iterate over the lists of each weapon to potentially find a match.

            if (!PlayerBackpackManager.TryGetItem(__instance.m_player, slot, out var bpItem)) return;
            uint bpID = bpItem.GearIDRange.GetOfflineID();

            foreach (var content in __instance.m_popupScrollWindow.ContentItems)
            {
                CM_InventorySlotItem slotItem = content.TryCast<CM_InventorySlotItem>()!;
                uint slotID = slotItem.m_gearID.GetOfflineID();
                if (!GearToggleManager.Current.TryGetRelatedIDs(slotID, out var relatedIDs)) continue;

                foreach (uint id in relatedIDs)
                {
                    if (bpID == id && GearManager.TryGetGear("OfflineGear_ID_" + id, out var idRange))
                    {
                        slotItem.IsPicked = true;
                        slotItem.LoadData(idRange, true, true);
                        __instance.OnWeaponSlotItemSelected(slotItem);
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.ShowWeaponSelectionPopup))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_LoadoutMenuOpened(CM_PlayerLobbyBar __instance)
        {
            CM_ScrollWindowInfoBox infoBox = __instance.m_popupScrollWindow.InfoBox;
            // Need to instantiate a new button every time since the window is instantiated every time
            SwapButton = GameObject.Instantiate(CM_PageLoadout.Current.m_copyLobbyIdButton.gameObject, infoBox.transform);
            CM_Item item = SwapButton.GetComponent<CM_Item>();
            item.m_texts[0].SetText("Switch Gear");
            item.transform.localPosition = new(-300, -320, -1);

            item.OnBtnPressCallback = null;
            item.add_OnBtnPressCallback((Action<int>)((id) =>
            {
                // Guaranteed to be non-null with related IDs since button is only active if so
                CM_InventorySlotItem slotItem = __instance.selectedWeaponSlotItem;
                uint offlineID = slotItem.m_gearID.GetOfflineID();
                List<uint> relatedIDs = GearToggleManager.Current.GetRelatedIDs(offlineID)!;

                uint nextID = relatedIDs[(relatedIDs.IndexOf(offlineID) + 1) % relatedIDs.Count];
                if (GearManager.TryGetGear("OfflineGear_ID_" + nextID, out var newRange))
                {
                    __instance.selectedWeaponSlotItem.LoadData(newRange, true, true);
                    __instance.OnWeaponSlotItemSelected(slotItem);
                }
                else
                    PWLogger.Error($"Couldn't swap to next weapon ({nextID}) in toggle list!");
            }));

            // Need to manually call this since it's not called in every case we need it to be
            if (__instance.selectedWeaponSlotItem != null)
                Post_LoadoutItemSelected(__instance.selectedWeaponSlotItem);
        }

        [HarmonyPatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.OnWeaponSlotItemSelected))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_LoadoutItemSelected(CM_InventorySlotItem slotItem)
        {
            if (SwapButton == null) return;

            uint id = slotItem.m_gearID.GetOfflineID();
            SwapButton.SetActive(GearToggleManager.Current.HasRelatedIDs(id));
        }
    }
}
