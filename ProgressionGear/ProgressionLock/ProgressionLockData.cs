using System.Collections.Generic;

namespace ProgressionGear.ProgressionLock
{
    public sealed class ProgressionLockData
    {
        public readonly static ProgressionLockData[] Template = new ProgressionLockData[]
        {
            new()
            {
                Unlock = new()
                {
                    new ProgressionRequirement()
                    {
                        LevelLayoutID = 420
                    },
                    new ProgressionRequirement()
                    {
                        Tier = eRundownTier.TierB,
                    },
                    new ProgressionRequirement()
                    {
                        LevelLayoutID = 10,
                        Main = true,
                        Secondary = true
                    },
                    new ProgressionRequirement()
                    {
                        Tier = eRundownTier.TierA,
                        TierIndex = 1,
                        Main = false,
                        All = true,
                    }
                },
                UnlockRequired = 2,
                Lock = new()
                {
                    new ProgressionRequirement()
                    {
                        Tier = eRundownTier.TierC
                    }
                },
                LockRequired = 0,
                OfflineIDs = new() {0},
                Priority = 0,
                Name = "Example"
            },
            new()
            {
                Name = "Empty Example"
            }
        };

        public List<ProgressionRequirement> Unlock { get; set; } = new();
        public int UnlockRequired = 0;
        public List<ProgressionRequirement> Lock { get; set; } = new();
        public int LockRequired = 0;
        public bool MissingLevelDefault { get; set; } = true;
        public List<uint> OfflineIDs { get; set; } = new();
        public int Priority { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
    }
}
