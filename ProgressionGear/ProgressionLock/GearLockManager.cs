using ProgressionGear.Utils;
using Gear;
using Player;
using System.Collections.Generic;
using System.Linq;

namespace ProgressionGear.ProgressionLock
{
    public sealed class GearLockManager
    {
        public static GearLockManager Instance { get; private set; } = new();

        public GearManager? VanillaGearManager { internal set; get; } // setup in patch: GearManager.SetupGearInOfflineMode


        private readonly HashSet<uint> _lockedGearIds = new();

        internal readonly List<(InventorySlot inventorySlot, Dictionary<uint, GearIDRange> loadedGears)> GearSlots = new() {
            (InventorySlot.GearStandard, new()),
            (InventorySlot.GearSpecial, new()),
            (InventorySlot.GearMelee, new()),
            (InventorySlot.GearClass, new()),
        };

        private bool _setupGearOnLoad = false;

        internal void Setup()
        {
            if (_setupGearOnLoad)
            {
                _setupGearOnLoad = false;
                SetupAllowedGearsForActiveRundown();
            }
        }

        private void ConfigRundownGears()
        {
            if (Configuration.DisableProgression)
                return;

            _lockedGearIds.Clear();
            GearToggleManager.Current.ResetRelatedIDs();

            // Acquire all progression-locked/-unlocked weapons
            IEnumerator<ProgressionLockData> dataEnum = ProgressionLockManager.Current.GetEnumerator();
            Dictionary<uint, PriorityLockData> priorityIDs = new();
            while (dataEnum.MoveNext())
            {
                ProgressionLockData progData = dataEnum.Current;
                foreach (uint id in progData.OfflineIDs)
                {
                    PriorityLockData lockData = new()
                    {
                        priority = progData.Priority,
                        locked = IsGearLocked(progData),
                        explicitLock = IsLockExplicit(progData)
                    };

                    if (priorityIDs.TryGetValue(id, out var data))
                    {
                        // Resolve lock conflict by explicit vs implicit locks or, if both are the same, by priority.
                        if ((data.explicitLock && !lockData.explicitLock) || data.priority > lockData.priority)
                            continue;
                    }
                    priorityIDs.Add(id, lockData);
                }
            }

            // Store locked IDs to remove from gear lists later
            foreach (var kv in priorityIDs.Where(kv => kv.Value.locked))
            {
                _lockedGearIds.Add(kv.Key);
                GearToggleManager.Current.RemoveFromRelatedIDs(kv.Key);
            }
        }

        private struct PriorityLockData
        {
            public int priority;
            public bool locked;
            public bool explicitLock;
        }

        private void ClearLoadedGears()
        {
            foreach (var slot in GearSlots)
            {
                VanillaGearManager!.m_gearPerSlot[(int)slot.inventorySlot].Clear();
            }
        }

        public bool IsGearAllowed(uint id)
        {
            return !_lockedGearIds.Contains(id);
        }

        // Modifies the gear in each slot to only unlocked gear.
        // Appears to only affect loadouts. Excluded gear still remains in GearManager's registered data for everything.
        private void AddGearForCurrentRundown()
        {
            foreach (var (inventorySlot, loadedGears) in GearSlots)
            {
                var vanillaSlot = VanillaGearManager!.m_gearPerSlot[(int)inventorySlot];

                if (loadedGears.Count == 0)
                {
                    PWLogger.Debug($"No gear has been loaded for {inventorySlot}.");
                    continue;
                }

                foreach (uint offlineGearPID in loadedGears.Keys)
                {
                    if (IsGearAllowed(offlineGearPID))
                    {
                        vanillaSlot.Add(loadedGears[offlineGearPID]);
                    }
                }

                if (vanillaSlot.Count == 0)
                {
                    PWLogger.Error($"No gear is allowed for {inventorySlot}, there must be at least 1 allowed gear!");
                    vanillaSlot.Add(loadedGears.First().Value);
                }
            }
        }

        private void ResetPlayerSelectedGears()
        {
            VanillaGearManager!.RescanFavorites();
            foreach (var gearSlot in GearSlots)
            {
                var inventorySlotIndex = (int)gearSlot.inventorySlot;

                if (VanillaGearManager.m_lastEquippedGearPerSlot[inventorySlotIndex] != null)
                    PlayerBackpackManager.EquipLocalGear(VanillaGearManager.m_lastEquippedGearPerSlot[inventorySlotIndex]);
                else if (VanillaGearManager.m_favoriteGearPerSlot[inventorySlotIndex].Count > 0)
                    PlayerBackpackManager.EquipLocalGear(VanillaGearManager.m_favoriteGearPerSlot[inventorySlotIndex][0]);
                else if (VanillaGearManager.m_gearPerSlot[inventorySlotIndex].Count > 0)
                    PlayerBackpackManager.EquipLocalGear(VanillaGearManager.m_gearPerSlot[inventorySlotIndex][0]);
            }
        }

        // Must be done separate from AddGearForCurrentRundown so that ResetPlayerSelectedGears can still see all allowed gear.
        private void RemoveToggleGears()
        {
            foreach (var slot in GearSlots)
            {
                var gearList = VanillaGearManager!.m_gearPerSlot[(int)slot.inventorySlot];

                for (int i = gearList.Count - 1; i >= 0; i--)
                    if (!GearToggleManager.Current.IsVisibleID(gearList[i].GetOfflineID()))
                        gearList.RemoveAt(i);
            }
        }

        internal void SetupAllowedGearsForActiveRundown()
        {
            if (VanillaGearManager == null)
            {
                _setupGearOnLoad = true;
                return;
            }

            if (!ProgressionWrapper.UpdateReferences()) return;

            ConfigRundownGears();
            ClearLoadedGears();
            AddGearForCurrentRundown();
            ResetPlayerSelectedGears();
            RemoveToggleGears();
        }

        // If there are unfinished unlock requirements, it is implicitly locked.
        // If unlock requirements are met, it can still be explicitly locked by lock requirements.
        private static bool IsGearLocked(ProgressionLockData data)
        {
            bool locked = (data.Unlock.Any() && !data.Unlock.All(IsComplete))
                       || (data.Lock.Any() && data.Lock.All(IsComplete));

            return locked;
        }

        // Returns whether the lock (or unlock) for some data is explicitly set due to fulfilled requirements.
        // Conversely, an implicit lock occurs when unlock requirements are set but no requirements are fulfilled.
        private static bool IsLockExplicit(ProgressionLockData data)
        {
            return data.Unlock.All(IsComplete)
                || (data.Lock.Any() && data.Lock.All(IsComplete));
        }

        private static bool IsComplete(ProgressionRequirement req) => ProgressionWrapper.IsComplete(req);
    }
}