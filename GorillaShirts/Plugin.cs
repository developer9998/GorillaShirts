using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Networking;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Models.UI;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GorillaShirts
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    internal class Plugin : BaseUnityPlugin
    {
        public static bool State;

        public static event Action<bool> OnStateChanged;

        public static new PluginInfo Info;

        public static new ManualLogSource Logger;

        public static ConfigEntry<CharacterPreference> StandCharacter;

        public static ConfigEntry<string> Favourites;

        public static ConfigEntry<EDefaultShirtMode> DefaultShirtMode;

        public void Awake()
        {
            Info = base.Info;
            Logger = base.Logger;

            Config.SaveOnConfigSet = true;

            Favourites = Config.Bind("Preferences", "Favourites", JsonConvert.SerializeObject(Enumerable.Empty<string>()), "A collection of shirts favourited by the player");

            var characters = Enum.GetValues(typeof(CharacterPreference)).Cast<CharacterPreference>().ToArray();
            StandCharacter = Config.Bind("Appearance", "Stand Character Identity", characters[UnityEngine.Random.Range(0, characters.Length)], "The gender identity of the character present at the shirt stand");

            DefaultShirtMode = Config.Bind("Appearance", "Default Shirt Mode", EDefaultShirtMode.None, "The method used for how shirts are worn by players without the mod, known as default shirts");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Constants.GUID);
            GorillaTagger.OnPlayerSpawned(() => DontDestroyOnLoad(new GameObject($"{Constants.Name} {Constants.Version}", typeof(NetworkSolution_RaiseEvent), typeof(DataManager), typeof(ShirtManager))));
        }

        public void OnEnable()
        {
            State = true;
            OnStateChanged?.Invoke(true);
        }

        public void OnDisable()
        {
            State = false;
            OnStateChanged?.Invoke(false);
        }
    }
}
