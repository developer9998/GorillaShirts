using BepInEx;
using Bepinject;
using GorillaShirts.Behaviors.Tools;
using GorillaShirts.Patches;
using HarmonyLib;
using System;
using System.Reflection;

namespace GorillaShirts
{
    [BepInDependency("dev.auros.bepinex.bepinject"), BepInIncompatibility("com.nachoengine.playermodel")]
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
            harmony.Patch(AccessTools.Method(rigCacheType, "Start", null, null), prefix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.Prepare)));
            harmony.Patch(AccessTools.Method(rigCacheType, "AddRigToGorillaParent", null, null), postfix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.AddPatch)));
            harmony.Patch(AccessTools.Method(rigCacheType, "RemoveRigFromGorillaParent", null, null), postfix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.RemovePatch)));
        }
    }
}
