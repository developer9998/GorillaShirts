using BepInEx;
using GorillaShirts.Behaviours;
using GorillaShirts.Tools;
using HarmonyLib;
using UnityEngine;

namespace GorillaShirts
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logging.Initialize(Logger);
            Configuration.Initialize(Config);

            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.Guid);

            GorillaTagger.OnPlayerSpawned(() => new GameObject(typeof(Main).FullName).AddComponent<Main>());
        }

    }
}
