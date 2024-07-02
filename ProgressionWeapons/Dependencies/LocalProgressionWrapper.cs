using GameData;
using LocalProgression;
using LocalProgression.Data;
using System.Collections.Generic;

namespace ProgressionWeapons.Dependencies
{
    internal static class LocalProgressionWrapper
    {
        public const string GUID = "Inas.LocalProgression";

        private static readonly Dictionary<uint, ExpeditionKey> _layoutToExpedition;
        private static readonly Dictionary<eRundownTier, List<ExpeditionKey>> _tierExpeditionKeys;
        public static uint CurrentRundownID;

        static LocalProgressionWrapper()
        {
            _layoutToExpedition = new Dictionary<uint, ExpeditionKey>();
            _tierExpeditionKeys = new Dictionary<eRundownTier, List<ExpeditionKey>>()
            {
                { eRundownTier.TierA, new() },
                { eRundownTier.TierB, new() },
                { eRundownTier.TierC, new() },
                { eRundownTier.TierD, new() },
                { eRundownTier.TierE, new() }
            };
            CurrentRundownID = 0;
        }

        private struct ExpeditionKey
        {
            public eRundownTier tier;
            public int expIndex;
        }

        public static void UpdateReferences()
        {
            uint rundownID = LocalProgressionManager.Current.ActiveRundownID();
            if (rundownID == 0 || CurrentRundownID == rundownID)
                return;

            CurrentRundownID = rundownID;
            _layoutToExpedition.Clear();
            RundownDataBlock block = RundownDataBlock.GetBlock(CurrentRundownID);

            AddTierLayouts(eRundownTier.TierA, block.TierA);
            AddTierLayouts(eRundownTier.TierB, block.TierB);
            AddTierLayouts(eRundownTier.TierC, block.TierC);
            AddTierLayouts(eRundownTier.TierD, block.TierD);
            AddTierLayouts(eRundownTier.TierE, block.TierE);
        }

        private static void AddTierLayouts(eRundownTier tier, Il2CppSystem.Collections.Generic.List<ExpeditionInTierData> dataList)
        {
            // Stores info needed to get expeditions for tiers and level layouts for easy access later
            _tierExpeditionKeys[tier].Clear();
            for (int i = 0; i < dataList.Count; i++)
            {
                ExpeditionInTierData data = dataList[i];
                if (!data.Enabled)
                    continue;

                ExpeditionKey key = new() { tier = tier, expIndex = i };
                _layoutToExpedition.Add(data.LevelLayoutData, key);
                _tierExpeditionKeys[tier].Add(key);
            }
        }

        private static ExpeditionProgressionData GetProgressionData(uint id, eRundownTier tier, int index)
        {
            return LocalProgressionManager.Current.GetExpeditionLP(id, tier, index);
        }

        private static bool ExpeditionComplete(ExpeditionProgressionData data)
        {
            return data.MainCompletionCount > 0 || data.SecondaryCompletionCount > 0 || data.ThirdCompletionCount > 0;
        }

        public static bool LayoutComplete(uint layoutID)
        {
            UpdateReferences();
            if (!_layoutToExpedition.ContainsKey(layoutID))
                return true;

            ExpeditionKey key = _layoutToExpedition[layoutID];
            return ExpeditionComplete(GetProgressionData(CurrentRundownID, key.tier, key.expIndex));
        }

        public static bool TierComplete(eRundownTier tier)
        {
            UpdateReferences();

            foreach(ExpeditionKey key in _tierExpeditionKeys[tier])
                if (!ExpeditionComplete(GetProgressionData(CurrentRundownID, key.tier, key.expIndex)))
                    return false;
            return true;
        }
    }
}
