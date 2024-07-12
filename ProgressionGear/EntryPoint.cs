using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProgressionGear.Utils;
using ProgressionGear.ProgressionLock;
using ProgressionGear.Dependencies;
using ProgressionGear.Patches;

namespace ProgressionGear;

[BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.1.2")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(LocalProgressionWrapper.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(EOSWrapper.GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal sealed class EntryPoint : BasePlugin
{
    public const string MODNAME = "ProgressionGear";

    public override void Load()
    {
        PWLogger.Log("Loading " + MODNAME);

        Harmony harmony = new(MODNAME);
        harmony.PatchAll();
        if (!LocalProgressionWrapper.HasLocalProgression)
            harmony.PatchAll(typeof(PageRundownPatches_NoLP));
        if (EOSWrapper.HasEOS)
            harmony.PatchAll(typeof(RundownManagerPatches_EOS));

        Configuration.Init();

        // Force objects to be initialized to create directories on launch
        GearToggleManager.Current.Init();
        ProgressionLockManager.Current.Init();

        PWLogger.Log("Loaded " + MODNAME);
    }
}