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

        private HumanoidContainer playerHumanoid;

        private readonly List<EShirtFallback> playerFallbacks = [];

        public void Start()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;

            playerHumanoid = gameObject.GetOrAddComponent<HumanoidContainer>();

            if (!HasGorillaShirts && Creator is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef)
                NetworkManager.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);

            // humanoid.WearShirt(Main.Instance.Shirts.GetRandomItem());
        }

        public void OnDestroy()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;

            if (playerHumanoid != null && playerHumanoid) Destroy(playerHumanoid);
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

                        playerHumanoid.OffsetNameTag(tagOffset);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error(ex);
                }

                try
                {
                    if (properties.TryGetValue("Fallbacks", out object fallbackObject) && fallbackObject is int[] fallbackArray)
                    {
                        for(int i = 0; i < fallbackArray.Length; i++)
                        {
                            int fallbackIndex = fallbackArray[i];
                            EShirtFallback fallback = Enum.IsDefined(typeof(EShirtFallback), fallbackIndex) ? (EShirtFallback)fallbackIndex : EShirtFallback.None;

                            if ((i + 1) > playerFallbacks.Count) playerFallbacks.Insert(i, fallback);
                            else playerFallbacks[i] = fallback;
                        }
                    }
                }
                catch(Exception)
                {

                }

                try
                {
                    if (properties.TryGetValue("Shirts", out object shirtsObject) && shirtsObject is string[] shirtPreferences)
                    {
                        Logging.Info($"Shirts: {string.Join(", ", shirtPreferences)}");

                        List<IGorillaShirt> shirtsToRemove = [];
                        List<IGorillaShirt> currentShirts = [.. playerHumanoid.Shirts];

                        foreach (IGorillaShirt shirt in currentShirts)
                        {
                            if (shirtPreferences.Length > 0 && shirtPreferences.Contains(shirt.ShirtId)) continue;
                            shirtsToRemove.Add(shirt);
                        }

                        List<IGorillaShirt> shirtsToWear = [];
                        for (int i = 0; i < shirtPreferences.Length; i++)
                        {
                            var preference = shirtPreferences[i];
                            if (playerHumanoid.Shirts.Any(shirt => shirt.ShirtId == preference)) continue;

                            IGorillaShirt shirt = null;
                            if (Main.Instance.Shirts.ContainsKey(preference)) shirt = Main.Instance.Shirts[preference];
                            else if (playerFallbacks.Count > i && playerFallbacks.ElementAtOrDefault(i) != EShirtFallback.None && Main.Instance.GetShirtFromFallback(playerFallbacks[i]) is IGorillaShirt fallbackShirt) shirt = fallbackShirt;

                            if (shirt is not null) shirtsToWear.Add(shirt);
                        }

                        if (shirtsToWear.Count > 0)
                        {
                            shirtsToWear.ForEach(playerHumanoid.UnionShirt);
                            Main.Instance.PlayShirtWearSound(playerHumanoid.Rig, shirts: [.. shirtsToWear]);
                        }
                        else if (shirtsToRemove.Count > 0)
                        {
                            shirtsToRemove.ForEach(playerHumanoid.NegateShirt);
                            Main.Instance.PlayShirtRemoveSound(playerHumanoid.Rig, shirts: [.. shirtsToRemove]);
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
