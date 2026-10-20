# ProgressionGear

Adds 3 features for rundown developers: **Gear Toggling**, **Progression-Locked Gear**, and **Level-Locked Gear**. These are read in via custom files under `Custom/ProgressionGear/GearToggle/`, `Custom/ProgressionGear/ProgressionLocks/`, and `Custom/ProgressionGear/LevelLocks/` respectively. If the directories do not exist, template files are created automatically.

## Gear Toggling

Gear Toggling allows gear to occupy the same slot in the loadout selection screen. When a gear with toggle data is selected, a button appears underneath its description that swaps to the next gear when clicked.

The custom files are lists of objects which contain:
- `OfflineIDs`: A list of PlayerOfflineGear IDs that are swapped between.
- `ButtonText`: The text to display on the button. Can specify two buttons in a list to move forward or backward in the list.
  - With two buttons, the right button follows single button direction while the left button follows the opposite.
- `ReverseOrder`: Reverses the direction buttons move through the list.
- `Name`: Serves no practical purpose, but can be handy for organizing/debugging as a developer.

The first ID in the list is the default weapon that appears; all others are hidden. Gear is swapped in the order of the list. The first ID in the list also determines the gear type (Main, Special, Tool, Melee). There are two rules for IDs:
- Unique: No ID can appear more than once across all lists.
- Same Slot: All IDs in the same list must be of the same gear type as the first ID.

If any IDs are progression or level locked, they are skipped when swapping weapons. If a list only has 1 or 0 valid IDs, no swap button will appear.

## Progression-Locked Gear

Progression-Locked Gear allows gear to be locked or unlocked as expeditions or tiers are completed. It is **not** used to lock gear to certain levels; Level-locked gear offers that instead.

The custom files are lists of objects which contain:
- `Unlock`: One or a list of level layout IDs, tiers, levels, or full requirement objects that must be completed to unlock the gear.
  - Layout IDs can be numbers or PartialData string IDs.
  - Tiers can be in the formats "TierA" or "B".
  - Levels are specified by Tier + Index (by 1), e.g. "TierA1" or "B3".
- `UnlockRequired`: The number of unlock requirements that must be completed to unlock the gear. If 0, requires all of them.
- `Lock`: A list of level layout IDs, tiers, levels, or full requirement objects that must be completed to lock the gear.
- `UnlockRequired`: The number of lock requirements that must be completed to lock the gear. If 0, requires all of them.
- `MissingLevelDefault`: Whether to count a level as completed if it does not exist.
- `OfflineIDs`: A list of PlayerOfflineGear IDs these unlocks/locks apply to.
- `Priority`: Specifies the priority of this unlock/lock. If two different blocks unlock/lock the same gear, the highest priority's unlock/lock is used.
- `RundownIDs`: One or a list of rundown IDs that these unlocks/locks apply to. If empty, applies to all rundowns.
- `Name`: Serves no practical purpose, but can be handy for organizing/debugging as a developer.

You may use a requirement object if you wish to specify sector completions. They contain the fields:

- `Level`: A level layout ID, tier, or level.
- `Main`, `Secondary`, `Overload`, `All`, `AllNoBoosters`: Requires the corresponding clear if set to true.
  - By default, only `Main` is required.

Any gear that has unlock requirements is automatically locked until they are completed. Additionally, lock requirements override unlock requirements in the event that both are completed.

Note: When resolving lock conflicts, completed requirements are considered a higher tier of priority. Empty unlock requirements are considered completed for these purposes. For instance, consider these two blocks:
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

## Level-Locked Gear

Level-Locked Gear restricts which gear can be used for expeditions or tiers. It operates similar to [ExtraObjectiveSetup](https://thunderstore.io/c/gtfo/p/Inas07/ExtraObjectiveSetup/) with additional features.

The custom files are lists of objects which contain:
- `Levels`: A list of level layout IDs, tiers, or levels that this lock applies to.
  - Layout IDs can be numbers or PartialData string IDs.
  - Tiers can be in the formats "TierA" or "B".
  - Levels are specified by Tier + Index (by 1), e.g. "TierA1" or "B3".
- `WhitelistIDs`: A list of PlayerOfflineGear IDs that are allowed to be used on the level. If null, all gear is allowed.
- `BlacklistIDs`: A list of PlayerOfflineGear IDs that are not allowed to be used on the level. If null, does not blacklist any gear.
- `BypassProgression`: Whether to allow weapons in `WhitelistIDs` regardless of progression locks.
- `LevelsToUnlock`: One or a list of level layout IDs, tiers, levels, or full requirement objects that must be completed to change to unlock whitelists/blacklists.
- `UnlockMode`: Specifies Unlock behavior.
  - `None`: Unlock behavior does not apply.
  - `PerLevel`: Each level in `Levels` is separately unlocked when it is cleared. Uses `LevelsToUnlock` as the requirement specification. If it is a list, uses the first element if it exists, otherwise a Main completion is required.
  - `List`: All of the level requirements in `LevelsToUnlock` must be cleared to unlock all `Levels`.
- `UnlockWhitelistIDs`: The whitelist to use when Unlock applies. `BypassProgression` only applies to IDs shared with `WhitelistIDs`.
- `UnlockBlacklistIDs`: The blacklist to use when Unlock applies.
- `UnlockRequired`: If `UnlockMode` uses `List`, the number of `LevelsToUnlock` requirements that must be completed to unlock `Levels`. If 0, requires all of them.
- `UnlockMissingLevelDefault`: Whether to count a level in `LevelsToUnlock` as completed if it does not exist.
- `RundownIDs`: One or a list of rundown IDs that this level-lock applies to. If empty, applies to all rundowns.
- `Name`: Serves no practical purpose, but can be handy for organizing/debugging as a developer.

`LevelsToUnlock` may use requirement objects if you wish to specify sector completions. They contain the fields:

- `Level`: A level layout ID, tier, or level. If `UnlockMode` is `PerLevel`, ignore this field.
- `Main`, `Secondary`, `Overload`, `All`, `AllNoBoosters`: Requires the corresponding clear if set to true.
  - By default, only `Main` is required.

Note: If a level is found in the `Levels` of two or more level-lock objects, only one will be used.
