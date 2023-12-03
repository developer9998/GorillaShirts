using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaShirts.Behaviours.Data;
using GorillaShirts.Behaviours.Interaction;
using GorillaShirts.Behaviours.Models;
using GorillaShirts.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace GorillaShirts.Behaviours.Tools
{
    public class Networking : MonoBehaviourPunCallbacks, IInitializable
    {
        public Hashtable myCustomProperties;

        private bool isPropertiesSwitching;
        private float propertySwitchTime;

        private Events events;
        private Dictionary<string, Shirt> _ShirtDict;
        private readonly Dictionary<VRRig, Rig> _RigDict = new();

        public void Initialize()
        {
            events = new Events();

            Events.RigAdded += delegate (Player player, VRRig vrRig)
            {
                if (player.IsLocal || vrRig.gameObject.GetComponent<RigInstance>() != null) return;
                CreateRigInstance(vrRig.gameObject, player);
            };
            Events.RigRemoved += delegate (Player player, VRRig vrRig)
            {
                if (player.IsLocal || !vrRig.TryGetComponent(out RigInstance rigInstance)) return;
                rigInstance.Rig.Remove();
                rigInstance.Rig.SetTagOffset(0);

                _RigDict.AddOrUpdate(vrRig, rigInstance.Rig);
                Destroy(rigInstance);
            };
            Events.CustomPropUpdate += delegate (Player player, Hashtable hashtable)
            {
                OnPlayerPropertiesUpdate(player, hashtable);
            };
        }

        public void Update()
        {
            if (isPropertiesSwitching && Time.unscaledTime > propertySwitchTime)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(myCustomProperties);

                isPropertiesSwitching = false;
                propertySwitchTime = Time.unscaledTime + Constants.NetworkCooldown;
            }
        }

        private RigInstance CreateRigInstance(GameObject gameObject, Player player)
        {
            if (gameObject.TryGetComponent(out RigInstance instanceCandidate)) return instanceCandidate;
            RigInstance rigInstance = gameObject.AddComponent<RigInstance>();

            rigInstance.Player = player;
            rigInstance.IsNetwork = true;
            rigInstance.Rig = _RigDict.TryGetValue(rigInstance.GetComponent<VRRig>(), out Rig rig) ? rig : null;

            return rigInstance;
        }

        public void UpdateProperties(Hashtable customProperties)
        {
            myCustomProperties = customProperties;
            isPropertiesSwitching = true;
        }

        public Hashtable GenerateHashtable(Shirt myShirt, int tagOffset) => new() { { Constants.ShirtKey, myShirt == null ? "None" : myShirt.Name }, { Constants.TagKey, tagOffset } };

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (targetPlayer.IsLocal) return;

            try
            {
                VRRig rigCandidate = GorillaParent.instance.vrrigDict.ContainsKey(targetPlayer) ? GorillaParent.instance.vrrigDict[targetPlayer] : null;
                VRRig targetRig = rigCandidate ?? RigUtils.GetRig(targetPlayer);
                RigInstance rigInstance = targetRig.gameObject.GetComponent<RigInstance>() ?? CreateRigInstance(targetRig.gameObject, targetPlayer);

                CheckPlayerProps(changedProps.ContainsKey(Constants.ShirtKey) ? changedProps : targetPlayer.CustomProperties, targetRig, rigInstance);
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
                _ShirtDict ??= ShirtUtils.ShirtDict;
                if (_ShirtDict.TryGetValue(shirtName, out Shirt myShirt))
                {
                    if (rigInstance.Rig.ActiveShirt != myShirt)
                    {
                        events.TriggerPlayShirtAudio(currentRig, 0, 0.5f);
                    }
                    rigInstance.Rig.Wear(myShirt);
                }
                else
                {
                    if (shirtName != "None" && !string.IsNullOrEmpty(shirtName) && rigInstance.Rig.ActiveShirt != null)
                    {
                        events.TriggerPlayShirtAudio(currentRig, 6, 0.6f);
                    }

                    if (rigInstance.Rig.ActiveShirt != myShirt)
                    {
                        events.TriggerPlayShirtAudio(currentRig, 1, 0.5f);
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
