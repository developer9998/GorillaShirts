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
        private bool setProperties;
        private float propertySetTimer;

        public override void OnEnable()
        {
            base.OnEnable();

            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

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

        public override void OnDisable()
        {
            base.OnDisable();

            if (Instance == this) Instance = null;
        }

        public void Update()
        {
            propertySetTimer -= Time.deltaTime;

            if (setProperties && properties.Count > 0 && propertySetTimer <= 0)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new()
                {
                    {
                        Constants.NetworkPropertyKey,
                        new Dictionary<string, object>(properties)
                    }
                });

                setProperties = false;
                propertySetTimer = Constants.NetworkRaiseInterval;
            }
        }

        public void SetProperty(string key, object value)
        {
            if (properties.ContainsKey(key)) properties[key] = value;
            else properties.Add(key, value);
            setProperties = true;
        }


        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            NetPlayer netPlayer = NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber);

            if (netPlayer.IsLocal || !VRRigCache.Instance.TryGetVrrig(netPlayer, out RigContainer playerRig) || !playerRig.TryGetComponent(out NetworkedPlayer networkedPlayer))
                return;

            object propertiesObject = null;

            if (changedProps.ContainsKey(Constants.NetworkPropertyKey))
            {
                Logging.Info($"{netPlayer.NickName} has GorillaShirts property");
                propertiesObject = changedProps[Constants.NetworkPropertyKey];
            }
            else if (changedProps.ContainsKey("ShirtProperties"))
            {
                Logging.Info($"{netPlayer.NickName} has ShirtProperties property");
                propertiesObject = changedProps["ShirtProperties"];
            }

            if (propertiesObject != null && propertiesObject is Dictionary<string, object> properties)
            {
                if (!networkedPlayer.HasGorillaShirts)
                {
                    Logging.Message($"{netPlayer.NickName} has GorillaShirts");
                    networkedPlayer.HasGorillaShirts = true;
                }

                OnPlayerPropertyChanged?.Invoke(netPlayer, properties);
                return;
            }
        }
    }
}
