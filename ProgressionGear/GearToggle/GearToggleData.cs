using System.Collections.Generic;

namespace ProgressionGear.ProgressionLock
{
    public sealed class GearToggleData
    {
        public List<uint> OfflineIDs { get; set; } = new();
        public Localization.LocalizedText ButtonText { get; set; } = new() { Id = 0, UntranslatedText = "Switch Gear" };
        public string Name { get; set; } = string.Empty;
    }
}
