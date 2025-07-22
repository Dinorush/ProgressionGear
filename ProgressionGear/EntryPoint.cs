﻿using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProgressionGear.Utils;
using ProgressionGear.ProgressionLock;
using ProgressionGear.Dependencies;
using ProgressionGear.Patches;
using Gear;

namespace ProgressionGear;

[BepInPlugin(GUID, MODNAME, "1.5.2")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(MTFOWrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(PartialDataWrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LocalProgressionWrapper.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(EOSWrapper.GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal sealed class EntryPoint : BasePlugin
{
    public const string GUID = "Dinorush." + MODNAME;
    public const string MODNAME = "ProgressionGear";

    public override void Load()
    {
        PWLogger.Log("Loading " + MODNAME);
        if (!MTFOWrapper.HasCustomContent)
        {
            PWLogger.Error("No MTFO datablocks detected. Not loading ProgressionGear...");
            return;
        }

        MTFO.API.MTFOHotReloadAPI.OnHotReload += FixGearInstanceDict;

        Harmony harmony = new(MODNAME);
        harmony.PatchAll();
        if (!LocalProgressionWrapper.HasLocalProgression)
            harmony.PatchAll(typeof(PageRundownPatches_NoLP));

        Configuration.Init();

        // Force objects to be initialized to create directories on launch
        GearLockManager.Current.Init();
        GearToggleManager.Current.Init();
        ProgressionLockManager.Current.Init();

        PWLogger.Log("Loaded " + MODNAME);
    }

    private static void FixGearInstanceDict()
    {
        GearManager.Current.m_allGearPerInstanceKey.Clear();
        GearManager.Current.OnGearLoadingDone();
    }
}