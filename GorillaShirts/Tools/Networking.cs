using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Models;
using GorillaShirts.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Tools
{
    public class Networking : MonoBehaviourPunCallbacks
    {
        public static Networking Instance;

        public Hashtable CustomProperties;

        private bool IsUpdatingProperties;
        private float PropertyUpdateTime;

        private readonly Dictionary<VRRig, Rig> Cache_RigInfo = [];

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            Logging.Info("Networking Awake");
        }

        public void Update()
        {
            if (IsUpdatingProperties && Time.unscaledTime > PropertyUpdateTime)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(CustomProperties);

                IsUpdatingProperties = false;
                PropertyUpdateTime = Time.unscaledTime + Constants.NetworkCooldown;
            }
        }

        public void UpdateProperties(Hashtable customProperties)
        {
            CustomProperties = customProperties;
            IsUpdatingProperties = true;
        }

        public Hashtable GenerateHashtable(Shirt myShirt, int tagOffset) => new()
        {
            { Constants.ShirtKey, myShirt == null ? "None" : myShirt.Name },
            { Constants.TagKey, tagOffset }
        };

        public void AddShirtRig(VRRig playerRig)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerRig.Creator.ActorNumber);
            Logging.Info($"Adding ShirtRig to player {player.NickName}");
            GetShirtRig(player, playerRig);
            Logging.Info("Added");
        }

        public void RemoveShirtRig(VRRig playerRig)
        {
            ShirtRig shirtRig = GetShirtRig(null, playerRig); // player argument is only needed for creating the shirtrig
            Player player = shirtRig.Player;

            Logging.Info($"Removing ShirtRig from player {player.NickName}");

            if (player.IsLocal)
            {
                Logging.Warning("Local player");
                return;
            }

            shirtRig.Rig.RemoveShirt();
            shirtRig.Rig.MoveNameTag();
            shirtRig.OnShirtRemoved();

            Cache_RigInfo.AddOrUpdate(playerRig, shirtRig.Rig);
            Destroy(shirtRig);

            Logging.Info("Removed");
        }

        public ShirtRig GetShirtRig(Player player, VRRig playerRig = null)
        {
            if (!playerRig)
            {
                playerRig = RigUtils.GetPlayerRig(player);
            }

            if (!playerRig.TryGetComponent(out ShirtRig shirtRig))
            {
                shirtRig = playerRig.AddComponent<ShirtRig>();
                shirtRig.Player = player;
                shirtRig.Rig = Cache_RigInfo.TryGetValue(shirtRig.GetComponent<VRRig>(), out Rig rig) ? rig : null;
            }

            return shirtRig;
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

            if (targetPlayer.IsLocal) return;

            VRRig targetRig = RigUtils.GetVRRig(targetPlayer);

            ShirtRig shirtRig = GetShirtRig(targetPlayer);

            try
            {
                // nametag
                if (changedProps.TryGetValue(Constants.TagKey, out object tagKey) && tagKey is int tagOffset)
                {
                    Logging.Info($"{targetPlayer.NickName} has a name tag offset of {tagOffset}");
                    shirtRig.Rig.NameTagOffset = tagOffset;
                }

                // shirt
                if (changedProps.TryGetValue(Constants.ShirtKey, out object shirtKey) && shirtKey is string wornGorillaShirt)
                {
                    if (Main.TotalInitializedShirts.ContainsKey(wornGorillaShirt))
                    {
                        Shirt newShirt = Main.TotalInitializedShirts[wornGorillaShirt];

                        Logging.Info($"{targetPlayer.NickName} is wearing shirt {newShirt.DisplayName}");

                        shirtRig.Rig.WearShirt(newShirt, out Shirt oldShirt);
                        shirtRig.Rig.MoveNameTag();

                        if (oldShirt == newShirt) return; // check for if a sound should be made

                        if (newShirt.Wear)
                        {
                            // play a custom shirt wearing audio
                            Main.Instance.PlayCustomAudio(targetRig, newShirt.Wear, 0.5f);
                        }
                        else
                        {
                            // play the default shirt wearing audio
                            Main.Instance.PlayShirtAudio(targetRig, 0, 0.5f);
                        }

                        return;
                    }

                    Shirt currentShirt = shirtRig.Rig.Shirt;

                    Logging.Info($"{targetPlayer.NickName} is removing shirt {currentShirt.DisplayName}");

                    if (wornGorillaShirt != "None" && !string.IsNullOrEmpty(wornGorillaShirt))
                    {
                        // play the shirt missing audio
                        Main.Instance.PlayShirtAudio(targetRig, 6, 1f);
                    }

                    shirtRig.Rig.RemoveShirt();
                    shirtRig.Rig.MoveNameTag();

                    if (shirtRig.Rig.Shirt == currentShirt) return; // check for if a sound should be made

                    if (shirtRig.Rig.Shirt != null && currentShirt.Remove)
                    {
                        // play a custom shirt removal audio
                        Main.Instance.PlayCustomAudio(targetRig, currentShirt.Remove, 0.5f);
                    }
                    else
                    {
                        // play the default shirt removal audio
                        Main.Instance.PlayShirtAudio(targetRig, 1, 0.5f);
                    }
                }
            }
            catch(Exception ex)
            {
                Logging.Error($"Failed to handle custom props from player {targetPlayer.NickName}: {ex}");
            }
        }

        public void SetShirt()
        {

        }

        public void SetTagOffset()
        {

        }
    }
}
