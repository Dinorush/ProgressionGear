using System.Collections.Generic;

namespace ProgressionGear.ProgressionLock
{
    public sealed class GearToggleData
    {
        public List<uint> OfflineIDs { get; set; } = new();
        public string Name { get; set; } = string.Empty;
    }
}
