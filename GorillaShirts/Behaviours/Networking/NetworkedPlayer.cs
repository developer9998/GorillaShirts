using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using Photon.Realtime;
using UnityEngine;

namespace GorillaShirts.Behaviours.Networking
{
    [RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
    public class NetworkedPlayer : MonoBehaviour
    {
        public bool HasGorillaShirts;

        public VRRig PlayerRig;

        public NetPlayer Owner;

        private PlayerShirtRig ShirtRig;

        public async Task Start()
        {
            ShirtRig = PlayerRig.gameObject.AddComponent<PlayerShirtRig>();
            ShirtRig.PlayerRig = PlayerRig;

            NetworkHandler.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;

            await Task.Delay(300);

            Player player = Owner.GetPlayerRef();
            NetworkHandler.Instance.OnPlayerPropertiesUpdate(player, player.CustomProperties);
        }

        public void OnDestroy()
        {
            NetworkHandler.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;

            if (HasGorillaShirts)
            {
                HasGorillaShirts = false;
                Destroy(ShirtRig);
            }
        }

        public void OnPlayerPropertyChanged(NetPlayer targetPlayer, Dictionary<string, object> properties)
        {
            if (targetPlayer == Owner)
            {
                Logging.Info($"{targetPlayer.NickName} got updated properties");

                if (properties.TryGetValue("TagOffset", out object tagOffsetObject) && tagOffsetObject is int tagOffset)
                {
                    Logging.Info($"Tag Offset: {tagOffset}");

                    ShirtRig.RigHandler.OffsetNameTag(tagOffset);
                }

                if (properties.TryGetValue("Shirts", out object shirtsObject) && shirtsObject is string[] shirt_names)
                {
                    Logging.Info($"Shirts: {string.Join(", ", shirt_names)}");

                    List<IShirtAsset> wornShirts = [], removedShirts = [];

                    var shirts = new List<IShirtAsset>(ShirtRig.RigHandler.Shirts);

                    foreach (var shirt in shirts)
                    {
                        if (shirt_names.Length > 0 && shirt_names.Contains(shirt.Descriptor.Name)) continue;
                        removedShirts.Add(shirt);
                    }

                    foreach (var shirt_name in shirt_names)
                    {
                        if (ShirtRig.RigHandler.ShirtNames.Contains(shirt_name) || !Main.Shirts.TryGetValue(shirt_name, out IShirtAsset shirt)) continue;
                        wornShirts.Add(shirt);
                    }

                    if (wornShirts.Count > 0)
                    {
                        int shirtVolume = wornShirts.Count;
                        float audioVolume = 1f / shirtVolume;

                        foreach (var shirt in wornShirts)
                        {
                            ShirtRig.RigHandler.WearShirt(shirt);
                            if (shirt.Descriptor.CustomWearSound)
                            {
                                Singleton<Main>.Instance.PlayCustomAudio(PlayerRig, shirt.Descriptor.CustomWearSound, 0.5f * audioVolume);
                            }
                            else
                            {
                                Singleton<Main>.Instance.PlayShirtAudio(PlayerRig, 0, 0.5f * audioVolume);
                            }
                        }
                    }
                    else if (removedShirts.Count > 0)
                    {
                        int shirtVolume = removedShirts.Count;
                        float audioVolume = 1f / shirtVolume;

                        foreach (var shirt in removedShirts)
                        {
                            ShirtRig.RigHandler.RemoveShirt(shirt);
                            if (shirt.Descriptor.CustomRemoveSound)
                            {
                                Singleton<Main>.Instance.PlayCustomAudio(PlayerRig, shirt.Descriptor.CustomRemoveSound, 0.5f * audioVolume);
                            }
                            else
                            {
                                Singleton<Main>.Instance.PlayShirtAudio(PlayerRig, EShirtAudio.ShirtRemove, 0.5f * audioVolume);
                            }
                        }
                    }
                }
            }
        }
    }
}
