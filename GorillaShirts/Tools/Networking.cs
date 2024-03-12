using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaShirts.Interaction;
using GorillaShirts.Models;
using GorillaShirts.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace GorillaShirts.Tools
{
    public class Networking : MonoBehaviourPunCallbacks, IInitializable
    {
        public Hashtable CustomProperties;

        private bool IsUpdatingProperties;
        private float PropertyUpdateTime;

        private Events InstancedEvents;
        private Dictionary<string, Shirt> Cache_ShirtInfo;
        private readonly Dictionary<VRRig, Rig> Cache_RigInfo = new();

        public void Initialize()
        {
            InstancedEvents = new Events();

            Events.RigAdded += delegate (NetPlayer player, VRRig vrRig)
            {
                if (player.IsLocal || vrRig.gameObject.GetComponent<RigInstance>() != null) return;
                CreateRigInstance(vrRig.gameObject, player);
            };

            Events.RigRemoved += delegate (NetPlayer player, VRRig vrRig)
            {
                if (player.IsLocal || !vrRig.TryGetComponent(out RigInstance rigInstance)) return;
                rigInstance.Rig.Remove();
                rigInstance.Rig.SetTagOffset(0);
                rigInstance.OnShirtRemoved();

                Cache_RigInfo.AddOrUpdate(vrRig, rigInstance.Rig);
                Destroy(rigInstance);
            };

            Events.CustomPropUpdate += delegate (Player player, Hashtable hashtable)
            {
                OnPlayerPropertiesUpdate(player, hashtable);
            };
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

        private RigInstance CreateRigInstance(GameObject gameObject, NetPlayer player)
        {
            if (gameObject.TryGetComponent(out RigInstance instanceCandidate)) return instanceCandidate;

            RigInstance rigInstance = gameObject.AddComponent<RigInstance>();
            if (player is PunNetPlayer pnp)
            {
                rigInstance.Player = pnp.playerRef;
            }

            rigInstance.IsNetwork = true;
            rigInstance.Rig = Cache_RigInfo.TryGetValue(rigInstance.GetComponent<VRRig>(), out Rig rig) ? rig : null;

            return rigInstance;
        }

        /// <summary>
        /// Prompt the class to update the player's custom properties. The properties are changed based on a given debounce, therefor it doesn't excessively set them right away
        /// </summary>
        /// <param name="customProperties">The hashtable representing the updated custom properties</param>
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

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (targetPlayer.IsLocal) return;

            try
            {
                NetPlayer netPlayer = NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber);

                if (netPlayer != null)
                {
                    VRRig rigCandidate = GorillaParent.instance.vrrigDict[netPlayer];
                    VRRig targetRig = rigCandidate ?? RigCacheUtils.GetField<VRRig>(netPlayer);

                    if (targetRig == rigCandidate)
                    {
                        Logging.Info("GorillaParent was utilized when finding VRRig for player " + targetPlayer.ToString());
                    }
                    else
                    {
                        Logging.Info("Reflection was utilized when finding VRRig for player " + targetPlayer.ToString());
                    }

                    RigInstance rigInstance = targetRig.gameObject.GetComponent<RigInstance>() ?? CreateRigInstance(targetRig.gameObject, netPlayer);

                    CheckPlayerProps(changedProps.ContainsKey(Constants.ShirtKey) ? changedProps : targetPlayer.CustomProperties, targetRig, rigInstance);
                }
            }
            catch
            {
                Logging.Error("Error attempting to get shirt properties from player " + targetPlayer.NickName + " (" + targetPlayer.UserId + ")");
            }
        }

        public void CheckPlayerProps(Hashtable changedProps, VRRig currentRig, RigInstance rigInstance)
        {
            if (changedProps.TryGetValue(Constants.ShirtKey, out object shirtKey) && shirtKey is string shirtName)
            {
                Cache_ShirtInfo ??= ShirtUtils.ShirtDict;

                if (Cache_ShirtInfo.TryGetValue(shirtName, out Shirt myShirt))
                {
                    bool uniqueShirt = rigInstance.Rig.ActiveShirt != myShirt;
                    rigInstance.Rig.Wear(myShirt);

                    if (uniqueShirt)
                    {
                        InstancedEvents.TriggerPlayShirtAudio(currentRig, 0, 0.5f);
                        if (myShirt.Wear) InstancedEvents.TriggerPlayCustomAudio(currentRig, myShirt.Wear, 0.5f);
                    }
                }
                else
                {
                    if (shirtName != "None" && !string.IsNullOrEmpty(shirtName) && rigInstance.Rig.ActiveShirt != null)
                    {
                        InstancedEvents.TriggerPlayShirtAudio(currentRig, 6, 0.6f);
                    }

                    if (rigInstance.Rig.ActiveShirt != myShirt)
                    {
                        InstancedEvents.TriggerPlayShirtAudio(currentRig, 1, 0.5f);
                        if (rigInstance.Rig.ActiveShirt != null && rigInstance.Rig.ActiveShirt.Remove) InstancedEvents.TriggerPlayCustomAudio(currentRig, rigInstance.Rig.ActiveShirt.Remove, 0.5f);
                    }

                    rigInstance.Rig.Remove();
                    rigInstance.Rig.SetTagOffset(0);
                }
            }

            if (changedProps.TryGetValue(Constants.TagKey, out object tagKey) && tagKey is int tagOffset)
            {
                rigInstance.Rig.SetTagOffset(rigInstance.Rig.ActiveShirt == null ? 0 : tagOffset);
            }
        }
    }
}
