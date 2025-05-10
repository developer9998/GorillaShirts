using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GorillaShirts.Tools;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GorillaShirts.Behaviours.Networking
{
    public class NetworkHandler : Singleton<NetworkHandler>, IInRoomCallbacks
    {
        public Action<NetPlayer, Dictionary<string, object>> OnPlayerPropertyChanged;

        private readonly Dictionary<string, object> properties = [];
        private bool set_properties = false;
        private float properties_timer;

        public override void Initialize()
        {
            if (NetworkSystem.Instance && (NetworkSystem.Instance.CurrentPhotonBackend == "PUN" || NetworkSystem.Instance is NetworkSystemPUN))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new()
                {
                    { Constants.NetworkVersionKey, Constants.Version }
                });
                PhotonNetwork.AddCallbackTarget(this);
                Application.quitting += () => PhotonNetwork.RemoveCallbackTarget(this);
                return;
            }

            enabled = false; // either no netsys or not in a pun environment - i doubt fusion will ever come
        }

        public void FixedUpdate()
        {
            properties_timer -= Time.deltaTime;

            if (set_properties && properties.Count > 0 && properties_timer <= 0)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new()
                {
                    { Constants.NetworkPropertiesKey, new Dictionary<string, object>(properties) }
                });
                set_properties = false;
                properties_timer = Constants.NetworkSetInterval;
            }
        }

        public void SetProperty(string key, object value)
        {
            if (properties.ContainsKey(key)) properties[key] = value;
            else properties.Add(key, value);
            set_properties = true;
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            // https://github.com/The-Graze/Grate/blob/9dddf2084a75f22cc45024f38d564f788db661d6/Networking/NetworkPropertyHandler.cs#L45C13-L45C100

            NetPlayer target_player = NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber);

            if (changedProps.TryGetValue(Constants.NetworkPropertiesKey, out object props_object) && props_object is Dictionary<string, object> properties)
            {
                bool is_local_player = target_player.IsLocal;
                // Logging.Info($"Recieved properties from {target_player.NickName}{(is_local_player ? " (local player)" : "")}: {string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]"))}");

                if (!is_local_player)
                {
                    if (VRRigCache.Instance.TryGetVrrig(targetPlayer, out var rigContainer) && rigContainer.TryGetComponent(out NetworkedPlayer networked_player) && !networked_player.HasGorillaShirts)
                    {
                        networked_player.HasGorillaShirts = true;
                        Logging.Info($"{target_player.NickName} has GorillaShirts");
                    }

                    OnPlayerPropertyChanged?.Invoke(target_player, properties);
                }
            }
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {

        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {

        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {

        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {

        }
    }
}
