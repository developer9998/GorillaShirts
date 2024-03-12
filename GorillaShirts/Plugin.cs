using BepInEx;
using Bepinject;
using GorillaShirts.Patches;
using GorillaShirts.Tools;
using HarmonyLib;
using System;

namespace GorillaShirts
{
    [BepInDependency("dev.auros.bepinex.bepinject"), BepInIncompatibility("org.iidk.gorillatag.iimenu")] // yeah no..
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    internal class Plugin : BaseUnityPlugin
    {
        public Plugin()
        {
            // Prepare a selection of tools the mod uses
            Logging.Initalize(Constants.Name);

            // Prepare our installer with Bepinject
            Zenjector.Install<MainInstaller>().OnProject().WithConfig(Config).WithLog(Logger);

            // Prepare our harmony instance
            Harmony harmony = new(Constants.Guid);
            harmony.PatchAll(typeof(Plugin).Assembly);

            // Add some patches manually since they're under the internal and private keywords
            Type rigCacheType = typeof(GorillaTagger).Assembly.GetType("VRRigCache");
            harmony.Patch(AccessTools.Method(rigCacheType, "Start"), prefix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.Prepare)));
            harmony.Patch(AccessTools.Method(rigCacheType, "AddRigToGorillaParent"), postfix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.AddPatch)));
            harmony.Patch(AccessTools.Method(rigCacheType, "RemoveRigFromGorillaParent"), postfix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.RemovePatch)));
        }
    }
}
