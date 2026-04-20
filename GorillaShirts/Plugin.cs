using GorillaLibrary;
using GorillaShirts;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Networking;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Models.UI;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

[assembly: MelonInfo(typeof(Plugin), "GorillaShirts", "2.4.5", "dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
[assembly: MelonAdditionalDependencies("GorillaLibrary")]

namespace GorillaShirts;

internal class Plugin : GorillaMod
{
    public static MelonPreferences_Entry<CharacterPreference> StandCharacter;

    public static MelonPreferences_Entry<string> Favourites;

    public static MelonPreferences_Entry<EDefaultShirtMode> DefaultShirtMode;

    public override void OnInitializeMelon()
    {
        MelonPreferences_Category category = CreateCategory("GorillaShirts");

        Favourites = category.CreateEntry("favourites", JsonConvert.SerializeObject(Enumerable.Empty<string>()), "Favourites", "The collection of shirts favourited by the player", false, false, null);

        var characters = Enum.GetValues(typeof(CharacterPreference)).Cast<CharacterPreference>().ToArray();
        StandCharacter = category.CreateEntry("identity", characters[UnityEngine.Random.Range(0, characters.Length)], "Stand Character Identity", "The gender identity of the character present at the shirt stand", false, false, null);

        DefaultShirtMode = category.CreateEntry("defaultMode", EDefaultShirtMode.None, "Default Shirt Mode", "The method used for how shirts are worn by players without the mod, known as default shirts", false, false, null);

        Events.Core.OnGameInitialized.Subscribe(Initialize);
        Events.Rig.OnRigAdded.Subscribe(RigAdded);
        Events.Rig.OnRigRemoved.Subscribe(RigRemoved);
    }
    public void Initialize()
    {
        GameObject root = new("GorillaShirts", typeof(NetworkSolution_RaiseEvent), typeof(DataManager), typeof(ShirtManager), typeof(ThreadingUtility));
        UnityEngine.Object.DontDestroyOnLoad(root);
    }

    public void RigAdded(VRRig rig, NetPlayer player)
    {
        if (rig.GetComponent<NetworkedPlayer>()) return;

        NetworkedPlayer component = rig.gameObject.AddComponent<NetworkedPlayer>();
        component.PlayerRig = rig;
        component.Creator = player;
    }

    public void RigRemoved(VRRig rig)
    {
        if (rig.TryGetComponent(out NetworkedPlayer component))
        {
            UnityEngine.Object.Destroy(component);
        }
    }
}
