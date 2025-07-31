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
    internal class Main : MonoBehaviour
    {
        public static Main Instance { get; private set; }
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

        public async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            GameObject standObject = Instantiate(await AssetLoader.LoadAsset<GameObject>("ShirtStand"));
            standObject.name = "Shirt Stand";
            standObject.transform.SetParent(transform);

            ShirtStand = standObject.GetComponent<Stand>();

            ShirtStand.welcomeMenuRoot.SetActive(false);
            ShirtStand.loadMenuRoot.SetActive(false);
            ShirtStand.versionMenuRoot.SetActive(false);
            ShirtStand.mainMenuRoot.SetActive(false);
            ShirtStand.packBrowserMenuRoot.SetActive(false);

            ShirtStand.mainContentRoot.SetActive(true);
            ShirtStand.infoContentRoot.SetActive(false);

            ShirtStand.Character.SetAppearence(Plugin.StandCharacter.Value);

            ShirtStand.Character.OnPreferenceSet += delegate (ECharacterPreference preference)
            {
                ShirtStand.sillyHeadObject.SetActive(preference == ECharacterPreference.Feminine);
                ShirtStand.steadyHeadObject.SetActive(preference == ECharacterPreference.Masculine);
                PlayShirtAudio(preference switch
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

            ShirtStand.Character.gameObject.SetActive(false);

            foreach (EAudioType audioType in Enum.GetValues(typeof(EAudioType)).Cast<EAudioType>())
            {
                AudioClip audioClip = await AssetLoader.LoadAsset<AudioClip>(audioType.GetName());
                Audio.Add(audioType, audioClip);
            }

            RigContainer localRig = VRRigCache.Instance.localRig;
            LocalHumanoid = localRig.gameObject.AddComponent<HumanoidContainer>();
            // LocalHumanoid.Rig = localRig.Rig;

            MenuStateMachine = new StateMachine<Menu_StateBase>();
            menuState_Load = new Menu_Loading(ShirtStand);
            MenuStateMachine.SwitchState(new Menu_Welcome(ShirtStand));

            using UnityWebRequest request = UnityWebRequest.Get(@"https://raw.githubusercontent.com/developer9998/GorillaShirts/master/Packs/Packs.json");
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                Releases = request.downloadHandler.text.FromJson<ReleaseInfo[]>();
                Logging.Info($"Releases include: {string.Join(", ", Releases.OrderBy(info => info.Rank).Select(info => info.Title))}");
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
                string latestVersionRaw = request.downloadHandler.text.Trim();
                if (Version.TryParse(latestVersionRaw, out Version latestVersion) && latestVersion > Plugin.Info.Metadata.Version)
                {
                    PlayShirtAudio(EAudioType.Error, 1f);
                    TaskCompletionSource<object> completionSource = new(TaskCreationOptions.LongRunning);
                    MenuStateMachine.SwitchState(new Menu_WrongVersion(ShirtStand, Constants.Version, latestVersionRaw, completionSource));
                    await completionSource.Task;
                }
            }

            ShirtStand.Character.gameObject.SetActive(true);

            MenuStateMachine.SwitchState(menuState_Load);
            Content = new ContentHandler(Path.GetDirectoryName(Plugin.Info.Location));
            Content.LoadStageCallback += menuState_Load.SetLoadAppearance;
            Content.OnContentLoaded += OnContentLoaded;
            Content.LoadContent();
        }

        public void OnContentLoaded(List<PackDescriptor> content)
        {
            bool isInitialList = Packs is null;

            if (Packs == null) Packs = content;
            else Packs.AddRange(content);

            Packs = [.. Packs.OrderByDescending(pack => pack.Shirts.Max(shirt => shirt.FileInfo.LastWriteTime)).OrderBy(x => x.PackName switch
            {
                "Favourites" => 0,
                "Default" => 1,
                "Custom" => 2,
                _ => 10
            })];

            List<string> wornGorillaShirts = GetShirtNames(Plugin.ShirtPreferences);

            List<(IGorillaShirt shirt, PackDescriptor pack)> shirtsToWear = [];

            foreach (PackDescriptor packDescriptor in content)
            {
                if (packDescriptor.PackName == "Default") packDescriptor.Shuffle();
                else packDescriptor.Shirts.Sort((x, y) => x.Descriptor.ShirtName.CompareTo(y.Descriptor.ShirtName));

                for (int i = 0; i < packDescriptor.Shirts.Count; i++)
                {
                    if (Shirts.ContainsKey(packDescriptor.Shirts[i].ShirtId))
                    {
                        packDescriptor.Shirts.RemoveAt(i);
                        i--;
                        continue;
                    }
                    Shirts.Add(packDescriptor.Shirts[i].ShirtId, packDescriptor.Shirts[i]);

                    if (wornGorillaShirts != null && wornGorillaShirts.Contains(packDescriptor.Shirts[i].ShirtId))
                    {
                        shirtsToWear.Add((packDescriptor.Shirts[i], packDescriptor));
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
                AdjustTagOffset(Plugin.TagOffsetPreference.Value);

                FavouritePack = ScriptableObject.CreateInstance<PackDescriptor>();
                FavouritePack.PackName = "Favourites";
                FavouritePack.Author = null;
                FavouritePack.Description = "This is a special pack reserved for all of your favourite shirts!<br><br>To add or remove a shirt from your favourites, press the favourite button on the top right when viewing the shirt.";

                Packs.Insert(0, FavouritePack);
            }

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

            if (NetworkSystem.Instance.InRoom && GorillaParent.instance is GorillaParent gorillaParent)
            {
                foreach (VRRig rig in gorillaParent.vrrigs)
                {
                    if (rig.TryGetComponent(out NetworkedPlayer component) && component.Creator is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef)
                    {
                        NetworkManager.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);
                    }
                }
            }
        }

        public void Update()
        {
            MenuStateMachine?.Update();
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
                PlayShirtAudio(EAudioType.NegativeClick, 0.75f);
            }
            else
            {
                FavouritePack.Shirts.Add(shirt);
                PlayShirtAudio(EAudioType.PositiveClick, 0.75f);
            }

            SetShirtNames(FavouritePack.Shirts, Plugin.Favourites);
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

            SetShirtNames(LocalHumanoid.Shirts, Plugin.ShirtPreferences);
            NetworkShirts(LocalHumanoid.Shirts);
        }

        private void NetworkShirts(List<IGorillaShirt> shirts)
        {
            var fallbacks = shirts.Select(shirt => shirt.Descriptor.Fallback).Select(fallback => (int)fallback).ToArray();
            NetworkManager.Instance.SetProperty("Fallbacks", fallbacks);

            var shirtNames = shirts.Select(shirt => shirt.ShirtId).ToArray();
            NetworkManager.Instance.SetProperty("Shirts", shirtNames);
        }

        public void AdjustTagOffset(int tagOffset)
        {
            LocalHumanoid.OffsetNameTag(tagOffset);
            ShirtStand.Character.OffsetNameTag(tagOffset);
            Plugin.TagOffsetPreference.Value = tagOffset;
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
                if (shirt.Descriptor.WearSound is not null)
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
                if (shirt.Descriptor.RemoveSound is not null)
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

        public void PlayShirtAudio(EAudioType audio, float volume)
        {
            if (!Audio.TryGetValue(audio, out AudioClip audioClip)) return;
            PlayCustomAudio(audioClip, volume);
        }

        public void PlayCustomAudio(AudioClip clip, float volume)
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
