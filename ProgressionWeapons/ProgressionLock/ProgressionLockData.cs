using System.Collections.Generic;

namespace ProgressionWeapons.ProgressionLock
{
    public sealed class ProgressionLockData
    {
        public List<uint> UnlockLayoutIDs { get; set; } = new();
        public List<eRundownTier> UnlockTiers { get; set; } = new();
        public List<uint> LockLayoutIDs { get; set; } = new();
        public List<eRundownTier> LockTiers { get; set; } = new();
        public List<uint> OfflineIDs { get; set; } = new();
        public int Priority { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
    }
}
