using BepInEx.Unity.IL2CPP;
using LocalProgression;
using ProgressionGear.ProgressionLock;

namespace ProgressionGear.Dependencies
{
    internal class LocalProgressionWrapper
    {
        public const string GUID = "Inas.LocalProgression";

        public static bool HasLocalProgression { get; private set; }

        static LocalProgressionWrapper()
        {
            HasLocalProgression = IL2CPPChainloader.Instance.Plugins.ContainsKey(GUID);
        }

        public static ProgressionData GetProgressionDataLP(uint id, eRundownTier tier, int index)
        {
            if (!HasLocalProgression) return new ProgressionData();

            var dataLP = LocalProgressionManager.Current.GetExpeditionLP(id, tier, index);
            ProgressionData data = new(
                dataLP.ExpeditionKey,
                dataLP.MainCompletionCount,
                dataLP.SecondaryCompletionCount,
                dataLP.ThirdCompletionCount,
                dataLP.AllClearCount,
                dataLP.NoBoosterAllClearCount
                );
            return data;
        }
    }
}
