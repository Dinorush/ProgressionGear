﻿using BepInEx.Unity.IL2CPP;
using ProgressionGear.Utils;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace ProgressionGear.Dependencies
{
    internal static class PartialDataWrapper
    {
        public const string PLUGIN_GUID = "MTFO.Extension.PartialBlocks";
        public readonly static bool HasPartialData = false;

        public static JsonConverter? PersistentIDConverter { get; private set; } = null;

        static PartialDataWrapper()
        {
            if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
            {
                try
                {
                    var ddAsm = info?.Instance?.GetType()?.Assembly;
                    if (ddAsm is null)
                        throw new Exception("Assembly is Missing!");

                    var types = ddAsm.GetTypes();
                    var converterType = types.First(t => t.Name == "PersistentIDConverter");
                    if (converterType is null)
                        throw new Exception("Unable to Find PersistentIDConverter Class");

                    PersistentIDConverter = (JsonConverter)Activator.CreateInstance(converterType)!;
                    HasPartialData = true;
                }
                catch (Exception e)
                {
                    PWLogger.Error($"Exception thrown while reading data from MTFO_Extension_PartialData:\n{e}");
                }
            }
        }
    }
}
