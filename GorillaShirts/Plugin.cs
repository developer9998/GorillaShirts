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

namespace GorillaShirts;

[BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
[BepInDependency("dev.gorillalibrary")]
internal class Plugin : BaseUnityPlugin
{
    public static new PluginInfo Info;
    public static new ManualLogSource Logger;

    public static ConfigEntry<string> Favourites;

    public static ConfigEntry<CharacterPreference> StandCharacter;
    public static ConfigEntry<ShirtStandCullLayer> StandCulling;

    public static ConfigEntry<bool> ShirtUpdateShaders;
    public static ConfigEntry<bool> ShirtImportAudio, ShirtImportPhysics;

    public static ConfigEntry<EDefaultShirtMode> DefaultShirtMode;

    public void Awake()
    {
        Info = base.Info;
        Logger = base.Logger;

        Config.SaveOnConfigSet = true;

        // Preferences
        Favourites = Config.Bind("Preferences", "Favourites", JsonConvert.SerializeObject(Enumerable.Empty<string>()), "A collection of shirts favourited by the player");

        // Stand
        StandCulling = Config.Bind("Stand Appearance", "Stand Culling", ShirtStandCullLayer.All, "The culling bound to the shirt stand, meaning which perspectives can see the shirt stand");
        var characters = Enum.GetValues(typeof(CharacterPreference)).Cast<CharacterPreference>().ToArray();
        StandCharacter = Config.Bind("Stand Appearance", "Stand Character Identity", characters[UnityEngine.Random.Range(0, characters.Length)], "The gender identity of the character present at the shirt stand");

        // Shirt
        ShirtUpdateShaders = Config.Bind("Shirt Appearance", "Force Update Shaders", true, "Whether any instance of Gorilla Tag's generic shader found in shirts will update to what your game install uses or remain the same");
        ShirtImportAudio = Config.Bind("Shirt Mechanics", "Import Audio", false, "Whether any audio sources in shirts are imported as the shirts load in");
        ShirtImportPhysics = Config.Bind("Shirt Mechanics", "Import Physics", true, "Whether any physics related components in shirts are imported as the shirts load in");

        // Player
        DefaultShirtMode = Config.Bind("Player Appearance", "Default Shirt Mode", EDefaultShirtMode.None, "The method used for how shirts are worn by players without the mod, known as default shirts");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Constants.GUID);
        GorillaTagger.OnPlayerSpawned(() => DontDestroyOnLoad(new GameObject($"{Constants.Name} {Constants.Version}", typeof(NetworkSolution_RaiseEvent), typeof(DataManager), typeof(ShirtManager))));
    }
}
