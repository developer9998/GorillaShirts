using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaShirts.Interaction;
using GorillaShirts.Models;
using GorillaShirts.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Tools
{
    public class Networking : MonoBehaviourPunCallbacks
    {
        public Hashtable CustomProperties;

        private bool IsUpdatingProperties;
        private float PropertyUpdateTime;

        private Dictionary<string, Shirt> Cache_ShirtInfo;
        private readonly Dictionary<VRRig, Rig> Cache_RigInfo = [];

        public void Start()
        {
            Events.RigEnabled += (Player player, VRRig vrRig) =>
            {
                if (player.IsLocal || vrRig.gameObject.GetComponent<PhysicalRig>() != null) return;
                CreateRigInstance(vrRig, player);
            };

            Events.RigDisabled += (Player player, VRRig vrRig) =>
            {
                if (player.IsLocal || !vrRig.TryGetComponent(out PhysicalRig rigInstance)) return;

                rigInstance.Rig.Remove();
                rigInstance.Rig.SetTagOffset(0);
                rigInstance.OnShirtRemoved();

                Cache_RigInfo.AddOrUpdate(vrRig, rigInstance.Rig);
                Destroy(rigInstance);
            };

            Events.CustomPropUpdate += OnPlayerPropertiesUpdate;
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

        private PhysicalRig CreateRigInstance(VRRig targetRig, Player targetPlayer)
        {
            if (targetRig.TryGetComponent(out PhysicalRig instanceCandidate)) return instanceCandidate;

            PhysicalRig rigInstance = targetRig.AddComponent<PhysicalRig>();
            rigInstance.Player = targetPlayer;

            rigInstance.IsNetwork = true;
            rigInstance.Rig = Cache_RigInfo.TryGetValue(rigInstance.GetComponent<VRRig>(), out Rig rig) ? rig : null;

            return rigInstance;
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

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

            if (targetPlayer.IsLocal) return;

            VRRig targetRig = GorillaGameManager.StaticFindRigForPlayer(targetPlayer);

            targetRig.TryGetComponent(out PhysicalRig physicalRig);
            physicalRig ??= CreateRigInstance(targetRig, targetPlayer);

            try
            {
                if (changedProps.TryGetValue(Constants.ShirtKey, out object shirtKey) && shirtKey is string shirtName)
                {
                    Cache_ShirtInfo ??= ShirtUtils.ShirtDict;

                    if (Cache_ShirtInfo.TryGetValue(shirtName, out Shirt myShirt))
                    {
                        bool uniqueShirt = physicalRig.Rig.CurrentShirt != myShirt;
                        physicalRig.Rig.Wear(myShirt);

                        if (uniqueShirt)
                        {
                            Events.PlayShirtAudio?.Invoke(targetRig, 0, 0.5f);
                            if (myShirt.Wear) Events.PlayCustomAudio?.Invoke(targetRig, myShirt.Wear, 0.5f);
                        }
                    }
                    else
                    {
                        if (shirtName != "None" && !string.IsNullOrEmpty(shirtName) && physicalRig.Rig.CurrentShirt != null)
                        {
                            Events.PlayShirtAudio?.Invoke(targetRig, 6, 0.8f);
                        }

                        if (physicalRig.Rig.CurrentShirt != myShirt)
                        {
                            Events.PlayShirtAudio?.Invoke(targetRig, 1, 0.5f);
                            if (physicalRig.Rig.CurrentShirt != null && physicalRig.Rig.CurrentShirt.Remove) Events.PlayCustomAudio?.Invoke(targetRig, physicalRig.Rig.CurrentShirt.Remove, 0.5f);
                        }

                        physicalRig.Rig.Remove();
                        physicalRig.Rig.SetTagOffset(0);
                    }
                }

                if (changedProps.TryGetValue(Constants.TagKey, out object tagKey) && tagKey is int tagOffset)
                {
                    physicalRig.Rig.SetTagOffset(physicalRig.Rig.CurrentShirt == null ? 0 : tagOffset);
                }
            }
            catch
            {

            }
        }
    }
}
