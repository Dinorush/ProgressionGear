using BepInEx.Unity.IL2CPP;
using MTFO.Ext.PartialData.JsonConverters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProgressionGear.Dependencies
{
    internal static class PartialDataWrapper
    {
        public const string GUID = "MTFO.Extension.PartialBlocks";

        public static bool HasPartialData { get; private set; }

        static PartialDataWrapper()
        {
            HasPartialData = IL2CPPChainloader.Instance.Plugins.ContainsKey(GUID);
        }

        public static void AddIDConverter(IList<JsonConverter> list)
        {
            if (HasPartialData)
                Unsafe_AddIDConverter(list);
        }
        private static void Unsafe_AddIDConverter(IList<JsonConverter> list) => list.Add(new PersistentIDConverter());
    }
}
