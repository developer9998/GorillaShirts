using GorillaExtensions;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Tools;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaShirts.Behaviours.Networking
{
    [RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
    internal class NetworkedPlayer : MonoBehaviour
    {
        public bool HasGorillaShirts;

        public VRRig Rig;

        public NetPlayer Creator;

        private HumanoidContainer humanoid;

        public void Start()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;

            humanoid = gameObject.GetOrAddComponent<HumanoidContainer>();

            if (!HasGorillaShirts && Creator is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef)
                NetworkManager.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);

            // humanoid.WearShirt(Main.Instance.Shirts.GetRandomItem());
        }

        public void OnDestroy()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;

            if (humanoid != null && humanoid) Destroy(humanoid);
        }

        public void OnPlayerPropertyChanged(NetPlayer player, Dictionary<string, object> properties)
        {
            if (player == Creator)
            {
                Logging.Message($"{player.NickName} got properties");
                //Logging.Info(string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]")));

                try
                {
                    if (properties.TryGetValue("TagOffset", out object tagOffsetObject) && tagOffsetObject is int tagOffset)
                    {
                        Logging.Info($"Tag Offset: {tagOffset}");

                        humanoid.OffsetNameTag(tagOffset);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error(ex);
                }

                try
                {
                    if (properties.TryGetValue("Shirts", out object shirtsObject) && shirtsObject is string[] shirtPreferences)
                    {
                        Logging.Info($"Shirts: {string.Join(", ", shirtPreferences)}");

                        List<IGorillaShirt> shirtsToRemove = [];
                        List<IGorillaShirt> currentShirts = [.. humanoid.Shirts];
                        foreach (IGorillaShirt shirt in currentShirts)
                        {
                            if (shirtPreferences.Length > 0 && shirtPreferences.Contains(shirt.ShirtId)) continue;
                            shirtsToRemove.Add(shirt);
                        }

                        List<IGorillaShirt> shirtsToWear = [];
                        foreach (string preference in shirtPreferences)
                        {
                            if (humanoid.Shirts.Any(shirt => shirt.ShirtId == preference) || !Main.Instance.Shirts.TryGetValue(preference, out IGorillaShirt shirt)) continue;
                            shirtsToWear.Add(shirt);
                        }

                        if (shirtsToWear.Count > 0)
                        {
                            shirtsToWear.ForEach(humanoid.UnionShirt);
                            Main.Instance.PlayShirtWearSound(humanoid.Rig, shirts: [.. shirtsToWear]);
                        }
                        else if (shirtsToRemove.Count > 0)
                        {
                            shirtsToRemove.ForEach(humanoid.NegateShirt);
                            Main.Instance.PlayShirtRemoveSound(humanoid.Rig, shirts: [.. shirtsToRemove]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error(ex);
                }
            }
        }
    }
}
