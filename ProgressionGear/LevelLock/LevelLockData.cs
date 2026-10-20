using ProgressionGear.JSON;
using ProgressionGear.JSON.Converters;
using ProgressionGear.ProgressionLock;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProgressionGear.LevelLock
{
    public sealed class LevelLockData
    {
        public readonly static LevelLockData[] Template = new LevelLockData[]
        {
            new()
            {
                Levels = new[]
                {
                    new LevelTarget()
                    {
                        LevelLayoutID = 420
                    }
                },
                Name = "Level Layout ID"
            },
            new()
            {
                Levels = new[]
                {
                    new LevelTarget()
                    {
                        Tier = eRundownTier.TierA,
                        TierIndex = 0
                    }
                },
                LevelsToUnlock = new[]
                {
                    new ProgressionRequirement()
                    {
                        Tier = eRundownTier.TierA,
                        TierIndex = 1
                    },
                    new ProgressionRequirement()
                    {
                        Tier = eRundownTier.TierA,
                        TierIndex = 2
                    }
                },
                UnlockMode = LevelUnlockMode.List,
                Name = "TierIndex example"
            },
            new()
            {
                Levels = new[]
                {
                    new LevelTarget()
                    {
                        LevelLayoutID = 420
                    },
                    new LevelTarget()
                    {
                        Tier = eRundownTier.TierA,
                        TierIndex = 0
                    }
                },
                UnlockMode = LevelUnlockMode.PerLevel,
                BlacklistIDs = new() {0},
                Name = "List example",
            }
        };

        [JsonConverter(typeof(OptionalArrayConverter<LevelTarget>))]
        public LevelTarget[] Levels { get; set; } = Array.Empty<LevelTarget>();
        public HashSet<uint>? WhitelistIDs { get; set; } = null;
        public HashSet<uint>? BlacklistIDs { get; set; } = null;
        public bool BypassProgression { get; set; } = false;
        [JsonConverter(typeof(OptionalArrayConverter<ProgressionRequirement>))]
        public ProgressionRequirement[] LevelsToUnlock { get; set; } = Array.Empty<ProgressionRequirement>();
        public LevelUnlockMode UnlockMode { get; set; } = LevelUnlockMode.None;
        public HashSet<uint>? UnlockWhitelistIDs { get; set; } = null;
        public HashSet<uint>? UnlockBlacklistIDs { get; set; } = null;
        public int UnlockRequired { get; set; } = 0;
        public bool UnlockMissingLevelDefault { get; set; } = true;
        [JsonConverter(typeof(OptionalArrayConverter<uint>))]
        public uint[] RundownIDs { get; set; } = Array.Empty<uint>();
        public string Name { get; set; } = string.Empty;
    }
}
