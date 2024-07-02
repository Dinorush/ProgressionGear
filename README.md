# ProgressionGear

Adds 2 features for rundown developers: **Gear Toggling** and **Progression-Locked Gear**. These are read in via custom files under `Custom/ProgressionWeapons/GearToggle/` and `Custom/ProgressionWeapons/ProgressionLocks/` respectively. If the directories do not exist, template files are created automatically.

## Gear Toggling

Gear Toggling allows gear to occupy the same slot in the loadout selection screen. When such a gear with toggle data is selected, a button appears underneath its description that swaps to the next gear when clicked.

The custom files are lists of objects which contain:
- `OfflineIDs`: A list of PlayerOfflineGear IDs that are swapped between.
- `Name`: Serves no practical purpose, but can be handy for organizing/debugging as a developer.

Gear is swapped in the order of the list. The first ID in the list is the default weapon that appears; swapping goes to the second and so on. The first ID in the list also determines the gear type (Main, Special, Tool, Melee). There are two rules for IDs:
- Unique: No ID can appear more than once across all lists.
- Same Slot: All IDs must be of the same gear type as the first ID.

If any IDs are progression locked, they are skipped when swapping weapons. If a list only has 1 or 0 valid IDs, no swap button will appear.

## Progression-Locked Gear

Progression-Locked Gear allows gear to be locked or unlocked as expeditions or tiers are completed. It is **not** used to lock gear to certain levels; [ExtraObjectiveSetup](https://thunderstore.io/c/gtfo/p/Inas07/ExtraObjectiveSetup/) offers that instead.

The custom files are lists of objects which contain:
- `UnlockLayoutIDs`: A list of level layout IDs that must be completed to unlock the gear.
- `UnlockTiers`: A list of tiers that must be completed to unlock the gear.
    - Can be "TierA", "TierB", ... or 1, 2, ...
- `LockLayoutIDs`: A list of level layout IDs that must be completed to lock the gear.
- `LockTiers`: A list of tiers that must be completed to lock the gear.
    - Can be "TierA", "TierB", ... or 1, 2, ...
- `OfflineIDs`: A list of PlayerOfflineGear IDs these unlocks/locks apply to.
- `Priority`: Specifies the priority of this unlock/lock. If two different blocks lock/unlock the same gear, the highest priority's lock/unlock is used.
- `Name`: Serves no practical purpose, but can be handy for organizing/debugging as a developer.

Any gear that has unlock requirements is automatically locked until they are completed. Additionally, lock requirements override unlock requirements in the event that both are completed. If layout IDs *and* tiers are required to unlock/lock, both must be completed.

Can work on mods with multiple rundowns, but has limited support. Requirements are only checked against the current rundown. If a level layout ID is not found, it is assumed to be completed.

Note: When resolving lock conflicts, completed requirements are considered a higher tier of priority. For instance, consider these two blocks:
```
{
  "UnlockTiers": ["TierA"],
  "LockTiers": ["TierB"],
  "Priority": 0
},
{
  "UnlockTiers": ["TierB"],
  "Priority": 1
}
```
Upon clearing A tier, the gear is unlocked since the first block's completed requirements give it a higher priority tier than the second. Once B tier is completed, the gear still remains unlocked since both blocks have the same priority tier and the second block has higher priority.
