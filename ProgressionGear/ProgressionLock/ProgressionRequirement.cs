namespace ProgressionGear.ProgressionLock
{
    public sealed class ProgressionRequirement
    {
        public eRundownTier Tier { get; set; } = eRundownTier.Surface;
        public int TierIndex { get; set; } = -1;
        public uint LevelLayoutID { get; set; } = 0;
        public bool Main { get; set; } = true;
        public bool Secondary { get; set; } = false;
        public bool Overload { get; set; } = false;
        public bool All { get; set; } = false;
        public bool AllNoBooster { get; set; } = false;

        public bool Complete(ProgressionData data)
        {
            return (!AllNoBooster || data.NoBoosterAllClearCount > 0)
                && (!All || data.AllCompletionCount > 0)
                && (!Main || data.MainCompletionCount > 0)
                && (!Secondary || data.SecondaryCompletionCount > 0)
                && (!Overload || data.ThirdCompletionCount > 0);
        }
    }
}
