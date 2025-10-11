using BepInEx;
using BepInEx.Configuration;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Behaviours.Networking;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Models;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Models.StateMachine;
using GorillaShirts.Models.UI;
using GorillaShirts.Tools;
using Newtonsoft.Json;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaShirts.Behaviours
{
    internal class ShirtManager : MonoBehaviour
    {
        public static ShirtManager Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        public Stand ShirtStand = null;

        public Dictionary<EAudioType, AudioClip> Audio = [];

        public ContentHandler Content;

        public ReleaseInfo[] Releases;

        public List<PackDescriptor> Packs;

        public PackDescriptor FavouritePack;

        public Dictionary<string, IGorillaShirt> Shirts = [];

        public HumanoidContainer LocalHumanoid;

        public StateMachine<Menu_StateBase> MenuStateMachine;

        private Menu_Loading menuState_Load;
        private Menu_PackCollection menuState_PackList;

        private bool initialized;

        public event Action<bool> OnPacksLoadedEvent;

        public async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            GameObject standObject = Instantiate(await AssetLoader.LoadAsset<GameObject>(Constants.StandAssetName));
            standObject.name = "Shirt Stand";
            standObject.transform.SetParent(transform);

            ShirtStand = standObject.GetComponent<Stand>();

            ShirtStand.welcomeMenuRoot.SetActive(false);
            ShirtStand.loadMenuRoot.SetActive(false);
            ShirtStand.versionMenuRoot.SetActive(false);
            ShirtStand.mainMenuRoot.SetActive(false);
            ShirtStand.packBrowserMenuRoot.SetActive(false);
            ShirtStand.packBrowserNewSymbol.SetActive(false);
            ShirtStand.mainContentRoot.SetActive(true);
            ShirtStand.infoContentRoot.SetActive(false);
            ShirtStand.mainMenu_colourSubMenu.SetActive(false);

            ShirtStand.Character.SetAppearence(Plugin.StandCharacter.Value);
            ShirtStand.Character.WearSignatureShirt();

            ShirtStand.Character.OnPreferenceSet += delegate (ECharacterPreference preference)
            {
                ShirtStand.mainSideBar.sillyHeadObject.SetActive(preference == ECharacterPreference.Feminine);
                ShirtStand.mainSideBar.steadyHeadObject.SetActive(preference == ECharacterPreference.Masculine);
                PlayAudio(preference switch
                {
                    ECharacterPreference.Masculine => EAudioType.SteadySpeech,
                    ECharacterPreference.Feminine => EAudioType.SillySpeech,
                    _ => EAudioType.Error
                }, 1f);
                Plugin.StandCharacter.Value = preference;
            };

            ShirtStand.Character.OnShirtWornEvent += delegate ()
            {
                bool invisibility = ShirtStand.Character.BodyType == EShirtBodyType.Invisible;

                ShirtStand.Character.MainSkin.forceRenderingOff = invisibility;
                ShirtStand.Character.Head.Find("Face").gameObject.SetActive(!invisibility);
                ShirtStand.Character.Body.Find("Chest").gameObject.SetActive(!invisibility);

                ShirtStand.Character.femAccessory.forceRenderingOff = invisibility || ShirtStand.Character.Shirts.Any(shirt => shirt.Objects.HasFlag(EShirtObject.Head));
                ShirtStand.Character.mascAccessory.forceRenderingOff = invisibility || ShirtStand.Character.Shirts.Any(shirt => shirt.Objects.HasFlag(EShirtObject.Head));

                GameObject[] allShirtObjects = ShirtStand.Character.Objects?.SelectMany(selector => selector.Value).Where(gameObject => gameObject.activeSelf).ToArray();
                for (int i = 0; i < allShirtObjects.Length; i++)
                {
                    AudioSource[] audioDevices = allShirtObjects[i].GetComponentsInChildren<AudioSource>();
                    if (audioDevices.Length > 0)
                    {
                        for (int k = 0; k < audioDevices.Length; k++)
                        {
                            Destroy(audioDevices[k]);
                        }
                    }
                }
            };

            foreach (EAudioType audioType in Enum.GetValues(typeof(EAudioType)).Cast<EAudioType>())
            {
                AudioClip audioClip = await AssetLoader.LoadAsset<AudioClip>(audioType.GetName());
                Audio.Add(audioType, audioClip);
            }

            RigContainer localRig = VRRigCache.Instance.localRig;
            LocalHumanoid = localRig.gameObject.AddComponent<HumanoidContainer>();

            MenuStateMachine = new StateMachine<Menu_StateBase>();
            menuState_Load = new Menu_Loading(ShirtStand);
            MenuStateMachine.SwitchState(new Menu_Welcome(ShirtStand));

            using UnityWebRequest request = UnityWebRequest.Get(@"https://raw.githubusercontent.com/developer9998/GorillaShirts/main/Packs/Packs.json");
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                Releases = request.downloadHandler.text.FromJson<ReleaseInfo[]>();
                Logging.Info($"Releases include: {string.Join(", ", Releases.OrderBy(info => info.Rank).Select(info => info.Title))}");

                Version pluginVersion = Plugin.Info.Metadata.Version;

                foreach (ReleaseInfo info in Releases)
                {
                    Version minimumVersion = info.MinimumPluginVersion is not null ? info.MinimumPluginVersion : pluginVersion;
                    info.IsOutdated = minimumVersion > pluginVersion;

                    string previewLink = info.PackPreviewLink;
                    if (string.IsNullOrEmpty(previewLink)) continue;

                    Texture2D texture = await AssetLoader.LoadTexture(previewLink);
                    if (texture is null || !texture) continue;

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    info.PreviewSprite = sprite;
                }
            }

            if (CosmeticsV2Spawner_Dirty.completed)
            {
                Initialize();
                return;
            }

            CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs += Initialize;
        }

        public async void Initialize()
        {
            if (initialized) return;
            initialized = true;

            using UnityWebRequest request = UnityWebRequest.Get(@"https://raw.githubusercontent.com/developer9998/GorillaShirts/main/Version.txt");
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                Version installedVersion = Plugin.Info.Metadata.Version;
                string latestVersionRaw = request.downloadHandler.text.Trim();
                if (Version.TryParse(latestVersionRaw, out Version latestVersion) && latestVersion > installedVersion)
                {
                    PlayAudio(EAudioType.Error, 1f);

                    bool nonPatchUpdate = latestVersion.Major > installedVersion.Major || latestVersion.Minor > installedVersion.Minor;
                    ShirtStand.hardVersionContainer.SetActive(nonPatchUpdate);
                    ShirtStand.softVersionContainer.SetActive(!nonPatchUpdate);

                    TaskCompletionSource<object> completionSource = new();
                    MenuStateMachine.SwitchState(new Menu_WrongVersion(ShirtStand, Constants.Version, latestVersionRaw, completionSource));

                    if (nonPatchUpdate) return;
                    await completionSource.Task;
                }
            }

            MenuStateMachine.SwitchState(menuState_Load);
            Content = new ContentHandler(Path.GetDirectoryName(Plugin.Info.Location));
            Content.ContentProcessCallback += menuState_Load.SetLoadAppearance;
            Content.OnPacksLoaded += OnPacksLoaded;
            Content.OnPackUnloaded += OnPackUnloaded;
            Content.OnShirtUnloaded += OnShirtUnloaded;
            Content.LoadContent();

            Plugin.DefaultShirtMode.SettingChanged += (sender, args) => ForEachNetworkedPlayer(player => player.AddDefaultShirt());
        }

        public void OnPacksLoaded(List<PackDescriptor> content)
        {
            Logging.Message($"Loaded {content.Count} packs");
            content.ForEach(pack => Logging.Info($"{pack.PackName}: {pack.Shirts.Count} shirts"));

            bool isInitialList = Packs is null;
            if (isInitialList) Packs = content;
            else Packs.AddRange(content);

            Packs = [.. Packs.Where(pack => pack.Shirts.Count != 0).OrderByDescending(pack => pack.Shirts.Max(shirt => shirt.FileInfo.LastWriteTime)).OrderBy(x => x.PackName switch
            {
                "Favourites" => 0,
                "Default" => 1,
                "Custom" => 2,
                _ => 10
            })];

            string[] wornGorillaShirts = DataManager.Instance.GetItem<string[]>("ShirtPreferences", []);

            List<(IGorillaShirt shirt, PackDescriptor pack)> shirtsToWear = [];

            foreach (PackDescriptor packDescriptor in content)
            {
                if (packDescriptor.PackName == "Default") packDescriptor.Shuffle();
                else packDescriptor.Shirts.Sort((x, y) => x.Descriptor.ShirtName.CompareTo(y.Descriptor.ShirtName));

                for (int i = 0; i < packDescriptor.Shirts.Count; i++)
                {
                    IGorillaShirt shirt = packDescriptor.Shirts[i];

                    if (Shirts.ContainsKey(shirt.ShirtId))
                    {
                        packDescriptor.Shirts.RemoveAt(i);
                        i--;
                        continue;
                    }
                    Shirts.Add(shirt.ShirtId, shirt);

                    LocalHumanoid.SetShirtColour(shirt, shirt.Colour);
                    ShirtStand.Character.SetShirtColour(shirt, shirt.Colour);

                    if (wornGorillaShirts != null && wornGorillaShirts.Contains(shirt.ShirtId))
                    {
                        shirtsToWear.Add((shirt, packDescriptor));
                        packDescriptor.Selection = i;
                    }
                }
            }

            if (shirtsToWear.Count > 0)
            {
                var shirts = (LocalHumanoid.Shirts ?? []).Concat(shirtsToWear.Select(tuple => tuple.shirt)).ToList();
                LocalHumanoid.SetShirts(shirts);
                PlayShirtWearSound(LocalHumanoid.Rig, shirts: [.. shirts]);
                NetworkShirts(shirts);
            }

            if (isInitialList)
            {
                AdjustTagOffset(DataManager.Instance.GetItem("TagOffset", 0));

                FavouritePack = ScriptableObject.CreateInstance<PackDescriptor>();
                FavouritePack.PackName = "Favourites";
                FavouritePack.Author = null;
                FavouritePack.Description = "This is a special pack reserved for all of your favourite shirts!<br><br>To add or remove a shirt from your favourites, press the favourite button on the top right when viewing the shirt.";
            }

            if (isInitialList || !Packs.Contains(FavouritePack)) Packs.Insert(0, FavouritePack);

            FavouritePack.Shirts.Clear();
            foreach (string shirtId in GetShirtNames(Plugin.Favourites))
            {
                if (Shirts.TryGetValue(shirtId, out IGorillaShirt shirt))
                {
                    FavouritePack.Shirts.Add(shirt);
                }
            }

            if (isInitialList)
            {
                menuState_PackList = new Menu_PackCollection(ShirtStand, Packs);
                MenuStateMachine.SwitchState(menuState_PackList);
            }
            else menuState_PackList.Packs = Packs;

            CheckPlayerProperties();

            OnPacksLoadedEvent?.Invoke(isInitialList);
        }

        public void OnShirtUnloaded(IGorillaShirt unloadedShirt)
        {
            if (!Shirts.ContainsKey(unloadedShirt.ShirtId)) return;

            Shirts.Remove(unloadedShirt.ShirtId);

            var shirtsToRemove = LocalHumanoid.Shirts.Where(wornShirt => wornShirt == unloadedShirt).ToList();
            if (shirtsToRemove.Count > 0)
            {
                var shirts = LocalHumanoid.Shirts.Except(shirtsToRemove).ToList();
                LocalHumanoid.SetShirts(shirts);
                PlayShirtRemoveSound(LocalHumanoid.Rig, shirts: [.. shirts]);
                NetworkShirts(shirts);
            }

            if (Packs.Find(pack => pack.Shirts.Contains(unloadedShirt)) is PackDescriptor pack)
            {
                pack.Shirts.Remove(unloadedShirt);
            }

            CheckPlayerProperties();
        }

        public void OnPackUnloaded(PackDescriptor content)
        {
            if (!Packs.Contains(content)) return;

            Packs.Remove(content);

            content.Shirts.Where(Shirts.ContainsValue).ForEach(async shirt =>
            {
                Shirts.Remove(shirt.ShirtId);
                await Content.UnloadShirt(shirt);
            });

            CheckPlayerProperties();

            if (content.Release is not null)
            {
                content.Release.Pack = null;
                content.Release = null;
            }

            Destroy(content);

            if (Packs.Count == 0)
            {
                ThreadingHelper.Instance.StartSyncInvoke(async () =>
                {
                    await Content.LoadDefaultContent(false);
                });
                return;
            }

            menuState_PackList.Packs = Packs;
            menuState_PackList.PreviewPack();
        }

        public void Update()
        {
            MenuStateMachine?.Update();
        }

        public void CheckPlayerProperties() => ForEachNetworkedPlayer(player => player.CheckProperties());

        private void ForEachNetworkedPlayer(Action<NetworkedPlayer> action)
        {
            if (!NetworkSystem.Instance.InRoom || !VRRigCache.isInitialized) return;

            foreach (RigContainer playerRig in VRRigCache.rigsInUse.Values)
            {
                if (!playerRig.TryGetComponent(out NetworkedPlayer component)) continue;
                action(component);
            }
        }

        public IGorillaShirt GetShirtFromFallback(EShirtFallback fallback)
        {
            if (fallback == EShirtFallback.None) return null;

            string shirtId = fallback switch
            {
                EShirtFallback.LongsleeveShirt => "Default/Longsleeve Shirt",
                EShirtFallback.Turtleneck => "Default/Turtleneck",
                EShirtFallback.TeeShirt => "Default/Tee Shirt",
                EShirtFallback.Hoodie => "Default/Hoodie",
                EShirtFallback.Overcoat => "Default/Overcoat",
                EShirtFallback.Croptop => "Default/Croptop",
                EShirtFallback.SillyCroptop => "Default/Silly's Croptop",
                EShirtFallback.SteadyHoodie => "Default/Steady's Hoodie",
                _ => null
            };

            return (!string.IsNullOrEmpty(shirtId) && Shirts.TryGetValue(shirtId, out IGorillaShirt shirt)) ? shirt : null;
        }

        public bool IsFavourite(IGorillaShirt shirt) => FavouritePack.Shirts.Contains(shirt);

        public void FavouriteShirt(IGorillaShirt shirt)
        {
            if (IsFavourite(shirt))
            {
                FavouritePack.Shirts.Remove(shirt);
                PlayAudio(EAudioType.NegativeClick, 0.75f);
            }
            else
            {
                FavouritePack.Shirts.Add(shirt);
                PlayAudio(EAudioType.PositiveClick, 0.75f);
            }

            SetShirtNames(FavouritePack.Shirts, Plugin.Favourites);
        }

        public void ColourShirt(IGorillaShirt shirt, Color colour, bool usePlayerColour)
        {
            shirt.Colour.CustomColour = colour;
            shirt.Colour.UsePlayerColour = usePlayerColour;
            shirt.Colour.SetData(shirt.ShirtId);
            LocalHumanoid.SetShirtColour(shirt, shirt.Colour);
            NetworkShirts(LocalHumanoid.Shirts);
        }

        public void HandleShirt(IGorillaShirt shirt)
        {
            if (LocalHumanoid.Shirts.Contains(shirt))
            {
                LocalHumanoid.NegateShirt(shirt);
                PlayShirtRemoveSound(LocalHumanoid.Rig, shirt);
            }
            else
            {
                LocalHumanoid.UnionShirt(shirt);
                PlayShirtWearSound(LocalHumanoid.Rig, shirt);
            }

            var shirtNames = LocalHumanoid.Shirts == null ? Enumerable.Empty<string>().ToArray() : [.. LocalHumanoid.Shirts.Select(shirt => shirt.ShirtId)];
            DataManager.Instance.SetItem("ShirtPreferences", shirtNames);
            NetworkShirts(LocalHumanoid.Shirts);
        }

        private void NetworkShirts(List<IGorillaShirt> shirts)
        {
            int[] fallbacks = [.. shirts.Select(shirt => shirt.Descriptor.Fallback).Select(fallback => fallback.GetIndex())];
            NetworkManager.Instance.SetProperty("Fallbacks", fallbacks);

            int[] colourData = [.. shirts.Select(shirt => shirt.Colour).Select(colour => colour.Data)];
            NetworkManager.Instance.SetProperty("Colours", colourData);

            string[] shirtNames = [.. shirts.Select(shirt => shirt.ShirtId)];
            NetworkManager.Instance.SetProperty("Shirts", shirtNames);
        }

        public void AdjustTagOffset(int tagOffset)
        {
            LocalHumanoid.OffsetNameTag(tagOffset);
            ShirtStand.Character.OffsetNameTag(tagOffset);
            DataManager.Instance.SetItem("TagOffset", tagOffset);
            NetworkTagOffset(tagOffset);
        }

        private void NetworkTagOffset(int tagOffset)
        {
            NetworkManager.Instance.SetProperty("TagOffset", tagOffset);
        }

        public void PlayShirtWearSound(VRRig playerRig, params IGorillaShirt[] shirts)
        {
            if (playerRig == null || shirts == null || shirts.Length == 0) return;

            int quantity = shirts.Length;
            float volume = 1 / (float)quantity;

            foreach (IGorillaShirt shirt in shirts)
            {
                if (shirt.Bundle && shirt.Descriptor && shirt.Descriptor.WearSound is not null)
                {
                    PlayCustomAudio(playerRig, shirt.Descriptor.WearSound, 0.5f * volume);
                    continue;
                }
                PlayShirtAudio(playerRig, EAudioType.ShirtWear, 0.5f * volume);
            }
        }

        public void PlayShirtRemoveSound(VRRig playerRig, params IGorillaShirt[] shirts)
        {
            if (playerRig == null || shirts == null || shirts.Length == 0) return;

            int quantity = shirts.Length;
            float volume = 1 / (float)quantity;

            foreach (IGorillaShirt shirt in shirts)
            {
                if (shirt.Bundle && shirt.Descriptor && shirt.Descriptor.RemoveSound is not null)
                {
                    PlayCustomAudio(playerRig, shirt.Descriptor.RemoveSound, 0.5f * volume);
                    continue;
                }
                PlayShirtAudio(playerRig, EAudioType.ShirtRemove, 0.5f * volume);
            }
        }

        public void PlayShirtAudio(VRRig playerRig, EAudioType audio, float volume)
        {
            if (!Audio.TryGetValue(audio, out AudioClip audioClip)) return;
            PlayCustomAudio(playerRig, audioClip, volume);
        }

        public void PlayCustomAudio(VRRig playerRig, AudioClip clip, float volume)
        {
            if (playerRig == null || clip == null) return;
            playerRig.tagSound.volume = 0.25f;
            playerRig.tagSound.GTPlayOneShot(clip, volume);
        }

        public void PlayOhNoAudio(float volume = 1f)
        {
            var enums = Enum.GetValues(typeof(EAudioType)).Cast<EAudioType>().Where(audioType => audioType.GetName().StartsWith("OhNo")).ToArray();
            PlayAudio(enums[UnityEngine.Random.Range(0, enums.Length)], volume);
        }

        public void PlayAudio(EAudioType audio, float volume = 1f)
        {
            if (!Audio.TryGetValue(audio, out AudioClip audioClip)) return;
            PlayAudio(audioClip, volume);
        }

        public void PlayAudio(AudioClip clip, float volume = 1f)
        {
            ShirtStand.AudioDevice.GTPlayOneShot(clip, volume);
        }

        private List<string> GetShirtNames(ConfigEntry<string> entry)
        {
            List<string> shirtNames = [];

            try
            {
                string[] shirtArray = JsonConvert.DeserializeObject<string[]>(entry.Value);
                shirtNames.AddRange(shirtArray);
            }
            catch (Exception)
            {
                entry.Value = JsonConvert.SerializeObject(Enumerable.Empty<string>());
            }

            return shirtNames;
        }

        private void SetShirtNames(IList<IGorillaShirt> shirts, ConfigEntry<string> entry)
        {
            var shirtNames = shirts == null ? Enumerable.Empty<string>().ToArray() : [.. shirts.Select(shirt => shirt.ShirtId)];
            entry.Value = JsonConvert.SerializeObject(shirtNames, Formatting.None);
        }
    }
}
