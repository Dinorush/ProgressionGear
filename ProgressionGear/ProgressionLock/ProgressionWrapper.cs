using GameData;
using ProgressionGear.Dependencies;
using System.Collections.Generic;

namespace ProgressionGear.ProgressionLock
{
    public static class ProgressionWrapper
    {
        private static readonly Dictionary<uint, ExpeditionKey> _layoutToExpedition;
        private static readonly Dictionary<eRundownTier, List<ExpeditionKey>> _tierExpeditionKeys;
        private static readonly Dictionary<eRundownTier, List<ProgressionData>> _nativeProgression;
        public static uint CurrentRundownID { get; private set; }

        static ProgressionWrapper()
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
            _nativeProgression = new Dictionary<eRundownTier, List<ProgressionData>>();
            CurrentRundownID = 0;
        }

        private struct ExpeditionKey
        {
            public eRundownTier tier;
            public int expIndex;
        }

        private static uint ActiveRundownID()
        {
            var rundownKey = RundownManager.ActiveRundownKey;
            if (!RundownManager.RundownProgressionReady || !RundownManager.TryGetIdFromLocalRundownKey(rundownKey, out var rundownID)) return 0u;

            return rundownID;
        }

        public static bool UpdateReferences()
        {
            uint rundownID = ActiveRundownID();
            if (rundownID == 0) return false;
            if (CurrentRundownID == rundownID) return true;

            CurrentRundownID = rundownID;
            _layoutToExpedition.Clear();
            RundownDataBlock block = RundownDataBlock.GetBlock(CurrentRundownID);

            AddTierLayouts(eRundownTier.TierA, block.TierA);
            AddTierLayouts(eRundownTier.TierB, block.TierB);
            AddTierLayouts(eRundownTier.TierC, block.TierC);
            AddTierLayouts(eRundownTier.TierD, block.TierD);
            AddTierLayouts(eRundownTier.TierE, block.TierE);

            return true;
        }

        internal static void UpdateNativeProgression(eRundownTier tier, int expIndex, ProgressionData data)
        {
            UpdateReferences();
            if (!_nativeProgression.ContainsKey(tier))
                _nativeProgression[tier] = new(5);

            List<ProgressionData> nativeDatas = _nativeProgression[tier];
            if (nativeDatas.Count <= expIndex)
            {
                nativeDatas.EnsureCapacity(expIndex + 1);
                for (int i = nativeDatas.Count; i <= expIndex; i++)
                    nativeDatas.Add(new());
            }
            nativeDatas[expIndex] = data;
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
                _layoutToExpedition.TryAdd(data.LevelLayoutData, key);
                _tierExpeditionKeys[tier].Add(key);
            }
        }

        private static ProgressionData GetProgressionData(uint id, eRundownTier tier, int index)
        {
            if (LocalProgressionWrapper.HasLocalProgression)
                return LocalProgressionWrapper.GetProgressionDataLP(id, tier, index);

            // Progression is being accessed on initial game load (before loading an individual rundown)
            // and I don't know how to stop it so this prevents the null ref
            if (_nativeProgression.Count == 0) return new ProgressionData();

            return _nativeProgression[tier][index];
        }

        public static bool IsComplete(ProgressionRequirement req)
        {
            UpdateReferences();
            if (req.LevelLayoutID != 0)
            {
                if (!_layoutToExpedition.ContainsKey(req.LevelLayoutID))
                    return true;
                ExpeditionKey key = _layoutToExpedition[req.LevelLayoutID];
                return req.Complete(GetProgressionData(CurrentRundownID, key.tier, key.expIndex));
            }
            else if (req.Tier != eRundownTier.Surface)
            {
                if (req.TierIndex >= 0 && req.TierIndex < _tierExpeditionKeys[req.Tier].Count)
                    return req.Complete(GetProgressionData(CurrentRundownID, req.Tier, req.TierIndex));

                foreach (ExpeditionKey key in _tierExpeditionKeys[req.Tier])
                    if (!req.Complete(GetProgressionData(CurrentRundownID, key.tier, key.expIndex)))
                        return false;
            }
            return true;
        }
    }
}
