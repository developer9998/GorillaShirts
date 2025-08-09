using ExitGames.Client.Photon;
using GorillaShirts.Tools;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Behaviours.Networking
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance { get; private set; }

        public Action<NetPlayer, Dictionary<string, object>> OnPlayerPropertyChanged;

        private readonly Dictionary<string, object> properties = [];
        private bool propertiesReady;
        private float propertySetTimer;

        public override void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            base.OnEnable();

            if (NetworkSystem.Instance is NetworkSystem netSys && netSys is NetworkSystemPUN)
            {
                SetProperty("Version", Constants.Version);

                PhotonNetwork.AddCallbackTarget(this);
                Application.quitting += delegate ()
                {
                    PhotonNetwork.RemoveCallbackTarget(this);
                };
                return;
            }

            enabled = false;
        }

        public void Update()
        {
            propertySetTimer = Mathf.Max(propertySetTimer - Time.unscaledDeltaTime, 0f);

            if (propertiesReady && propertySetTimer <= 0)
            {
                propertiesReady = false;
                propertySetTimer = Constants.NetworkRaiseInterval;

                PhotonNetwork.LocalPlayer.SetCustomProperties(new()
                {{
                    Constants.NetworkPropertyKey,
                    new Dictionary<string, object>(properties)
                }});
            }
        }

        public void SetProperty(string key, object value)
        {
            bool setProperties;

            if (properties.ContainsKey(key))
            {
                setProperties = !properties[key].Equals(value);
                properties[key] = value;
            }
            else
            {
                setProperties = true;
                properties.Add(key, value);
            }

            propertiesReady = propertiesReady || setProperties;
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            NetPlayer netPlayer = NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber);

            if (netPlayer.IsLocal || !VRRigCache.Instance.TryGetVrrig(netPlayer, out RigContainer playerRig) || !playerRig.TryGetComponent(out NetworkedPlayer networkedPlayer))
                return;

            object propertiesObject = null;

            if (changedProps.ContainsKey(Constants.NetworkPropertyKey))
            {
                propertiesObject = changedProps[Constants.NetworkPropertyKey];
            }

            if (propertiesObject is Dictionary<string, object> properties)
            {
                if (!networkedPlayer.IsShirtUser)
                {
                    Logging.Message($"Player has GorillaShirts: {targetPlayer.NickName}");
                    networkedPlayer.IsShirtUser = true;
                }

                OnPlayerPropertyChanged?.Invoke(netPlayer, properties);
                return;
            }
        }
    }
}
