using BepInEx;
using GorillaShirts.Behaviours;
using GorillaShirts.Patches;
using HarmonyLib;
using System;
using UnityEngine;

namespace GorillaShirts
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony _harmony;
        private readonly Type _vrRigCacheType;

        public Plugin()
        {
            _harmony = Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.Guid);
            _vrRigCacheType = typeof(GorillaTagger).Assembly.GetType("VRRigCache");

            _harmony.Patch(AccessTools.Method(_vrRigCacheType, "AddRigToGorillaParent"), postfix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.AddPatch)));
            _harmony.Patch(AccessTools.Method(_vrRigCacheType, "RemoveRigFromGorillaParent"), postfix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.RemovePatch)));

            GorillaTagger.OnPlayerSpawned(() => new GameObject("ShirtConstructor").AddComponent<ShirtConstructor>().ConfigFile = Config);
        }
    }
}
