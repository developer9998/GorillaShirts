using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GorillaExtensions;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Networking;
using GorillaShirts.Models.UI;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace GorillaShirts
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static new PluginInfo Info;
        public static new ManualLogSource Logger;

        public static ConfigEntry<ECharacterPreference> StandCharacter;
        public static ConfigEntry<string> ShirtPreferences;
        public static ConfigEntry<int> TagOffsetPreference;

        public void Awake()
        {
            Info = base.Info;
            Logger = base.Logger;

            Config.SaveOnConfigSet = true;
            StandCharacter = Config.Bind("Appearance", "Stand Character Identity", Enum.GetValues(typeof(ECharacterPreference)).Cast<ECharacterPreference>().ToList().GetRandomItem(), "The gender identity of the character present at the shirt stand");
            ShirtPreferences = Config.Bind("Preferences", "Shirts", string.Empty, "A collection of shirts worn by the player by name");
            TagOffsetPreference = Config.Bind("Preferences", "Tag Offset", 0, "The relative name tag offset applied to the player");

            Harmony.CreateAndPatchAll(typeof(Main).Assembly, Constants.GUID);
            GorillaTagger.OnPlayerSpawned(() => new GameObject(Constants.Name, typeof(NetworkManager), typeof(Main)));
        }
    }
}
