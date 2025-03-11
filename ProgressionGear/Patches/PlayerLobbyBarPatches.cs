using CellMenu;
using Gear;
using HarmonyLib;
using Player;
using ProgressionGear.Dependencies;
using ProgressionGear.ProgressionLock;
using ProgressionGear.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgressionGear.Patches
{
    [HarmonyPatch]
    internal static class PlayerLobbyBarPatches
    {
        // In some cases (join in progress) SetActiveExpedition doesn't seem to be called? Hopefully this fixes it.
        [HarmonyPatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.ShowWeaponSelectionPopup))]
        [HarmonyAfter(EOSWrapper.GUID)] // EOS doesn't patch this, but futureproofing JFS
        [HarmonyWrapSafe]
        [HarmonyPrefix]
        private static void Pre_ShowLoadoutForSlot()
        {
            uint lastID = ProgressionWrapper.CurrentRundownID;
            if (!ProgressionWrapper.UpdateReferences() || lastID == ProgressionWrapper.CurrentRundownID) return;

            GearLockManager.Current.SetupAllowedGearsForActiveRundown();
        }

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
            // Need to check each slot item to find which one the weapon belongs to.

            if (!PlayerBackpackManager.TryGetItem(__instance.m_player, slot, out var bpItem) || bpItem.GearIDRange == null) return;
            
            uint bpID = bpItem.GearIDRange.GetOfflineID();
            if (!GearToggleManager.Current.TryGetToggleInfo(bpID, out var toggleInfo)) return;

            foreach (var content in __instance.m_popupScrollWindow.ContentItems)
            {
                CM_InventorySlotItem slotItem = content.TryCast<CM_InventorySlotItem>()!;
                if (toggleInfo.ids.Contains(slotItem.m_gearID.GetOfflineID()))
                {
                    slotItem.IsPicked = true;
                    slotItem.LoadData(bpItem.GearIDRange, true, true);
                    __instance.OnWeaponSlotItemSelected(slotItem);
                    _cachedItem = __instance.selectedWeaponSlotItem;
                    return;
                }
            }
        }

        private static GameObject? _swapButton;
        private static TMPro.TextMeshPro? _swapText;

        [HarmonyPatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.ShowWeaponSelectionPopup))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_LoadoutMenuOpened(CM_PlayerLobbyBar __instance)
        {
            CM_ScrollWindowInfoBox infoBox = __instance.m_popupScrollWindow.InfoBox;
            // Need to instantiate a new button every time since the window is instantiated every time
            _swapButton = GameObject.Instantiate(CM_PageLoadout.Current.m_copyLobbyIdButton.gameObject, infoBox.transform);
            CM_Item item = _swapButton.GetComponent<CM_Item>();
            _swapText = item.m_texts[0];
            _swapText.SetText("Switch Gear");
            item.transform.localPosition = new(-300, -320, -1);

            item.OnBtnPressCallback = null;
            item.add_OnBtnPressCallback((Action<int>)((id) =>
            {
                CM_InventorySlotItem? slotItem = __instance.selectedWeaponSlotItem ?? _cachedItem;
                if (slotItem == null) return;

                uint offlineID = slotItem.m_gearID.GetOfflineID();
                List<uint> relatedIDs = GearToggleManager.Current.GetToggleInfo(offlineID)!.ids;

                uint nextID = relatedIDs[(relatedIDs.IndexOf(offlineID) + 1) % relatedIDs.Count];
                if (GearManager.TryGetGear("OfflineGear_ID_" + nextID, out var newRange))
                {
                    slotItem.LoadData(newRange, true, true);
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
            if (_swapButton == null) return;

            uint id = slotItem.m_gearID.GetOfflineID();

            if (GearToggleManager.Current.TryGetToggleInfo(id, out var toggleInfo))
            {
                _swapButton.SetActive(true);
                _swapText!.SetText(toggleInfo.text);
            }
            else
                _swapButton.SetActive(false);
        }
    }
}
