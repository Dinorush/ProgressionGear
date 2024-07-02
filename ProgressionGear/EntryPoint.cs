using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProgressionGear.Utils;
using ProgressionGear.Dependencies;
using ProgressionGear.ProgressionLock;

namespace ProgressionGear;

[BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.0.0")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(LocalProgressionWrapper.GUID, BepInDependency.DependencyFlags.HardDependency)]
internal sealed class EntryPoint : BasePlugin
{
    public const string MODNAME = "ProgressionGear";

    public override void Load()
    {
        PWLogger.Log("Loading " + MODNAME);

        new Harmony(MODNAME).PatchAll();
        Configuration.Init();

        // Force objects to be initialized to create directories on launch
        GearToggleManager.Current.Init();
        ProgressionLockManager.Current.Init();

        PWLogger.Log("Loaded " + MODNAME);
    }
}