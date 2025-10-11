using GorillaExtensions;
using GorillaShirts.Models;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Tools;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace GorillaShirts.Behaviours.Networking
{
    [RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
    internal class NetworkedPlayer : MonoBehaviour
    {
        public bool IsShirtUser;

        public VRRig PlayerRig;

        public NetPlayer Creator;

        public Player Creator_PunRef;

        private HumanoidContainer playerHumanoid;

        private readonly List<EShirtFallback> playerFallbacks = [];

        private readonly List<int> colourData = [];

        private Random irrelevantRandom, relevantRandom;

        private int? irrelevantIndex, relevantIndex;

        private bool hasDefaultShirt = false;

        public void Start()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;
            ShirtManager.Instance.OnPacksLoadedEvent += OnPacksLoaded;

            playerHumanoid = gameObject.GetOrAddComponent<HumanoidContainer>();

            Creator_PunRef = (Creator is PunNetPlayer punNetPlayer && punNetPlayer.PlayerRef is not null) ? punNetPlayer.PlayerRef : PhotonNetwork.CurrentRoom.GetPlayer(Creator.ActorNumber);

            if (!IsShirtUser)
            {
                irrelevantRandom = new();

                using SHA256 sha = SHA256.Create();
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Creator.UserId));
                int seed = BitConverter.ToInt32(hash, 0);
                relevantRandom = new(seed);

                AddDefaultShirt();
                CheckProperties();
            }

            // humanoid.WearShirt(Main.Instance.Shirts.GetRandomItem());
        }

        public void OnDestroy()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;
            ShirtManager.Instance.OnPacksLoadedEvent -= OnPacksLoaded;

            // finalize
            irrelevantRandom = null;
            relevantRandom = null;

            if (playerHumanoid != null && playerHumanoid) Destroy(playerHumanoid);
        }

        public void CheckProperties()
        {
            if (Creator_PunRef is null) return;
            NetworkManager.Instance.OnPlayerPropertiesUpdate(Creator_PunRef, Creator_PunRef.CustomProperties);
        }

        public void AddDefaultShirt() => OnPacksLoaded(true);

        private void OnPacksLoaded(bool isInitialList)
        {
            if (!isInitialList || IsShirtUser) return;

            if (Plugin.DefaultShirtMode.Value == EDefaultShirtMode.None)
            {
                RemoveDefaultShirt();
                return;
            }

            hasDefaultShirt = true;

            if (Plugin.DefaultShirtMode.Value == EDefaultShirtMode.Shared)
            {
                playerHumanoid.SetShirts(HumanoidContainer.LocalHumanoid.Shirts ?? []);
                return;
            }

            var shirts = ShirtManager.Instance.Shirts.Values;
            if (shirts == null || shirts.Count == 0) return;

            if (Plugin.DefaultShirtMode.Value == EDefaultShirtMode.RelevantPlayer && !relevantIndex.HasValue)
            {
                relevantIndex = relevantRandom.Next(0, shirts.Count);
            }

            if (Plugin.DefaultShirtMode.Value == EDefaultShirtMode.IrrelevantPlayer && !irrelevantIndex.HasValue)
            {
                irrelevantIndex = irrelevantRandom.Next(0, shirts.Count);
            }

            var index = (Plugin.DefaultShirtMode.Value == EDefaultShirtMode.RelevantPlayer ? relevantIndex : irrelevantIndex).GetValueOrDefault(0);
            playerHumanoid.SetShirt(shirts.ElementAt(index));
        }

        public void RemoveDefaultShirt()
        {
            if (!hasDefaultShirt) return;

            hasDefaultShirt = false;
            playerHumanoid.ClearShirts();
        }

        public void OnPlayerPropertyChanged(NetPlayer player, Dictionary<string, object> properties)
        {
            if (player == Creator)
            {
                Logging.Message($"{player.NickName}: Updated properties");

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
                        Dictionary<int, EShirtFallback> indexToEnumDict = EnumData<EShirtFallback>.Shared.IndexToEnum;

                        for (int i = 0; i < fallbackArray.Length; i++)
                        {
                            int fallbackIndex = fallbackArray[i];
                            EShirtFallback fallback = indexToEnumDict.ContainsKey(fallbackIndex) ? indexToEnumDict[fallbackIndex] : EShirtFallback.None;

                            if ((i + 1) > playerFallbacks.Count) playerFallbacks.Insert(i, fallback);
                            else playerFallbacks[i] = fallback;
                        }

                        Logging.Info($"Fallbacks: {string.Join(", ", playerFallbacks.Select(fallback => fallback.GetName()))}");
                    }
                }
                catch (Exception)
                {

                }

                try
                {
                    if (properties.TryGetValue("Colours", out object coloursObject) && coloursObject is int[] colourDataArray)
                    {
                        for (int i = 0; i < colourDataArray.Length; i++)
                        {
                            int data = colourDataArray[i];

                            if (i >= colourData.Count) colourData.Insert(i, data);
                            else colourData[i] = data;
                        }

                        Logging.Info($"Colour Data: {string.Join(", ", colourData)} (based on {string.Join(", ", colourDataArray)})");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Custom colours could not be set");
                    Logging.Error(ex);
                }

                try
                {
                    if (properties.TryGetValue("Shirts", out object shirtsObject) && shirtsObject is string[] shirtPreferences)
                    {
                        Logging.Info($"Shirts: {string.Join(", ", shirtPreferences)}");

                        if (hasDefaultShirt) RemoveDefaultShirt();

                        List<IGorillaShirt> shirtsToRemove = [];
                        List<IGorillaShirt> currentShirts = [.. playerHumanoid.Shirts];

                        foreach (IGorillaShirt shirt in currentShirts)
                        {
                            if (shirt.Bundle && ShirtManager.Instance.Shirts.ContainsValue(shirt) && shirtPreferences.Contains(shirt.ShirtId)) continue;
                            shirtsToRemove.Add(shirt);
                        }

                        if (shirtsToRemove.Count != 0) Logging.Info($"Removing: {string.Join(", ", shirtsToRemove.Select(shirt => shirt.ShirtId))}");

                        Dictionary<IGorillaShirt, int> shirtToColourData = [];

                        List<IGorillaShirt> shirtsToWear = [];

                        for (int i = 0; i < shirtPreferences.Length; i++)
                        {
                            var preference = shirtPreferences[i];

                            if (playerHumanoid.Shirts.Find(shirt => shirt.ShirtId == preference) is IGorillaShirt wornShirt && wornShirt.Bundle)
                            {
                                shirtToColourData.TryAdd(wornShirt, (i < 0 || i >= colourData.Count) ? -1 : colourData[i]);
                                continue;
                            }

                            IGorillaShirt shirt = null;

                            if (ShirtManager.Instance.Shirts.ContainsKey(preference))
                            {
                                shirt = ShirtManager.Instance.Shirts[preference];
                                Logging.Info($"{shirt.Descriptor?.ShirtName ?? shirt.ShirtId}");
                            }
                            else if (playerFallbacks.Count > i && playerFallbacks.ElementAtOrDefault(i) != EShirtFallback.None && ShirtManager.Instance.GetShirtFromFallback(playerFallbacks[i]) is IGorillaShirt fallbackShirt)
                            {
                                shirt = fallbackShirt;
                                Logging.Info($"{fallbackShirt.Descriptor?.ShirtName ?? fallbackShirt.ShirtId} (fallback)");
                            }

                            if (shirt is not null)
                            {
                                shirtsToWear.Add(shirt);
                                shirtToColourData.TryAdd(shirt, (i < 0 || i >= colourData.Count) ? -1 : colourData[i]);
                            }
                        }

                        if (shirtsToWear.Count != 0) Logging.Info($"Wearing: {string.Join(", ", shirtsToWear.Select(shirt => shirt.ShirtId))}");

                        if (shirtsToWear.Count > 0)
                        {
                            shirtsToWear.ForEach(playerHumanoid.UnionShirt);
                            ShirtManager.Instance.PlayShirtWearSound(playerHumanoid.Rig, shirts: [.. shirtsToWear]);
                        }
                        else if (shirtsToRemove.Count > 0)
                        {
                            shirtsToRemove.ForEach(playerHumanoid.NegateShirt);
                            ShirtManager.Instance.PlayShirtRemoveSound(playerHumanoid.Rig, shirts: [.. shirtsToRemove]);
                        }

                        //Logging.Info($"Colour Data: {shirtToColourData.Count} entries");

                        foreach (var (shirt, data) in shirtToColourData)
                        {
                            ShirtColour shirtColour = (ShirtColour)data;
                            //Logging.Info($"{shirt.Descriptor.ShirtName} Colour: {shirtColour}");

                            playerHumanoid.SetShirtColour(shirt, shirtColour);
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
