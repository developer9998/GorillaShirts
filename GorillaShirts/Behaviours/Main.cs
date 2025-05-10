using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Behaviours.Networking;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Buttons;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using GorillaShirts.Utilities;
using HarmonyLib;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Button = GorillaShirts.Behaviours.UI.Button;

namespace GorillaShirts.Behaviours
{
    public class Main : Singleton<Main>
    {
        public static Dictionary<string, IShirtAsset> Shirts = [];

        public PlayerShirtRig LocalRig;

        public Action<bool> SetInfoVisibility;

        public Action OnHierarchyChanged;

        public bool UseInfoPanel;

        public Stand Stand;

        private readonly List<IStandButton> buttons =
        [
            new ShirtEquip(),
            new ShirtIncrease(),
            new ShirtDecrease(),
            new RigToggle(),
            new Randomize(),
            new TagIncrease(),
            new TagDecrease(),
            new Information(),
            new Capture(),
            new Return()
        ];

        public int SelectedPackIndex;

        public List<Pack<IShirtAsset>> Packs = [];

        public Stack<Pack<IShirtAsset>> Hierarchy = [];

        public bool HasPack = false;

        public IShirtAsset SelectedShirt
        {
            get
            {
                var viewedPack = CurrentPack;
                return viewedPack?.Items?[viewedPack.Selection];
            }
            set
            {
                foreach (var pack in Packs)
                {
                    if (pack.Items.Contains(value))
                    {
                        pack.Selection = pack.Items.IndexOf(value);
                        CurrentPack = pack;
                        SelectedPack = pack;
                        break;
                    }
                }
            }
        }

        public Pack<IShirtAsset> SelectedPack
        {
            get => Packs[SelectedPackIndex];
            set => SelectedPackIndex = Packs.IndexOf(value);
        }

        public Pack<IShirtAsset> CurrentPack
        {
            get
            {
                if (Hierarchy.Count > 0 && Hierarchy.TryPeek(out var pack))
                {
                    return pack;
                }
                return null;
            }
            set
            {
                if (value != null && !Hierarchy.Contains(value))
                {
                    Hierarchy.Push(value);
                    OnHierarchyChanged?.Invoke();
                }
                else if (value == null && Hierarchy.Count > 0)
                {
                    Hierarchy.Pop();
                    OnHierarchyChanged?.Invoke();
                }
                HasPack = Hierarchy.Count > 0;
            }
        }

        public IStandNavigationInfo Selection => SelectedShirt is IStandNavigationInfo shirtNavInfo ? shirtNavInfo : SelectedPack;

        private ShirtLoader ShirtLoader;

        private readonly Dictionary<EShirtAudio, AudioClip> loadedAudio = [];

        private bool wrong_version = false;
        private string latest_version = "N/A";

        public async override void Initialize()
        {
#if RELEASE
            var requestVersion = UnityWebRequest.Get(@"https://raw.githubusercontent.com/developer9998/GorillaShirts/main/Version.txt");
            await TaskYieldUtils.Yield(requestVersion);

            if (requestVersion.result != UnityWebRequest.Result.Success)
            {
                Logging.Warning($"GitHub version string resulted with {requestVersion.result}: {requestVersion.downloadHandler.error}");
                wrong_version = true;
            }
            else
            {
                latest_version = requestVersion.downloadHandler.text.Trim();
                wrong_version = Constants.Version != latest_version;
                Logging.Warning($"GitHub version string returned {latest_version} (we are on {Constants.Version})");
                //return;
            }
#else
            wrong_version = false;
            latest_version = Constants.Version;
#endif

            await InitStand();
            Logging.Info("Shirt stand initialized");

            await InitAudio();
            Logging.Info("Shirt audio intialized");

            await InitCatalog();
            Logging.Info("Shirt catalog initialized");

            if (wrong_version)
            {
                Stand.Object.transform.Find("UI/LoadMenu").gameObject.SetActive(false);
                Stand.Object.transform.Find("UI/WrongVersionMenu").gameObject.SetActive(true);
                string version_correction = Stand.Object.transform.Find("UI/WrongVersionMenu/Text (3)").GetComponent<Text>().text
                    .Replace("[INSTALLEDVERSION]", Constants.Version)
                    .Replace("[CURRENTVERSION]", latest_version);
                Stand.Object.transform.Find("UI/WrongVersionMenu/Text (3)").GetComponent<Text>().text = version_correction;
                return;
            }

            if (Packs != null && Packs.Count > 0)
            {
                Stand.Display.UpdateDisplay(Selection, LocalRig.RigHandler.Shirts);
                Stand.Display.SetTag(Configuration.CurrentTagOffset.Value);
                Stand.Rig.OffsetNameTag(Configuration.CurrentTagOffset.Value);

                if (Hierarchy.Count > 0)
                {
                    List<IShirtAsset> shirts = [SelectedShirt];
                    shirts.AddRange(LocalRig.RigHandler.Shirts.Where(shirt => shirt != SelectedShirt && shirt.ComponentTypes.All(type => !SelectedShirt.ComponentTypes.Contains(type))));
                    Stand.Rig.Shirts = shirts;
                }
                else
                {
                    Stand.Rig.StartCycle(SelectedPack.Items);
                }

                Stand.Object.transform.Find("UI/LoadMenu").gameObject.SetActive(false);
                Stand.Object.transform.Find("UI/MainMenu").gameObject.SetActive(true);
                Stand.Object.transform.Find("UI/MainMenu/Text").gameObject.SetActive(true);
                Stand.Object.transform.Find("UI/MainMenu/Buttons").gameObject.SetActive(true);
            }

            if (NetworkSystem.Instance && NetworkSystem.Instance.InRoom)
            {
                foreach (var netPlayer in NetworkSystem.Instance.PlayerListOthers)
                {
                    if (netPlayer is PunNetPlayer punNetPlayer && punNetPlayer.PlayerRef is Player player)
                    {
                        Singleton<NetworkHandler>.Instance.OnPlayerPropertiesUpdate(player, player.CustomProperties);
                    }
                }
            }
        }

        public async Task InitStand()
        {
            LocalRig = GorillaTagger.Instance.offlineVRRig.gameObject.AddComponent<PlayerShirtRig>();

            GameObject shirtStand = Instantiate(await AssetLoader.LoadAsset<GameObject>("ShirtStand"));

            shirtStand.name = "Shirt Stand";
            shirtStand.transform.SetParent(transform);

            if (!shirtStand.TryGetComponent(out Stand))
            {
                Logging.Error($"ShirtStand is missing {nameof(UI.Stand)} component");
                return;
            }

            AudioSource standAudio = Stand.Audio;

            StandRigHandler standRig = new()
            {
                RigObject = shirtStand.transform.Find("Preview Gorilla").gameObject
            };
            standRig.RigObject.transform.Find("Rig").gameObject.AddComponent<Punch>();

            Stand.Camera.gameObject.SetActive(false);

            standRig.MainSkin = standRig.RigObject.GetComponentInChildren<SkinnedMeshRenderer>();
            standRig.StandNameTag = standRig.RigObject.GetComponentInChildren<Text>();
            standRig.Head = standRig.RigObject.transform.Find("Rig/body/head");
            standRig.Body = standRig.RigObject.transform.Find("Rig/body");
            standRig.LeftHand = standRig.RigObject.transform.Find("Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L");
            standRig.RightHand = standRig.RigObject.transform.Find("Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R");
            standRig.LeftLower = standRig.LeftHand.parent;
            standRig.RightLower = standRig.RightHand.parent;
            standRig.LeftUpper = standRig.LeftLower.parent;
            standRig.RightUpper = standRig.RightLower.parent;

            // Register the IK
            GorillaIKNonManaged gorillaIk = standRig.RigObject.transform.gameObject.AddComponent<GorillaIKNonManaged>();
            gorillaIk.targetLeft = shirtStand.transform.Find("Preview Gorilla/Rig/LeftTarget");
            gorillaIk.targetRight = shirtStand.transform.Find("Preview Gorilla/Rig/RightTarget");
            gorillaIk.leftUpperArm = standRig.LeftUpper;
            gorillaIk.leftLowerArm = standRig.LeftLower;
            gorillaIk.leftHand = standRig.LeftHand;
            gorillaIk.rightUpperArm = standRig.RightUpper;
            gorillaIk.rightLowerArm = standRig.RightLower;
            gorillaIk.rightHand = standRig.RightHand;

            standRig.OnAppearanceChange += delegate (Configuration.PreviewGorilla previewType)
            {
                shirtStand.transform.Find("UI/MainMenu/Sidebar/RigToggle/Silly").gameObject.SetActive(previewType == Configuration.PreviewGorilla.Silly);
                shirtStand.transform.Find("UI/MainMenu/Sidebar/RigToggle/Steady").gameObject.SetActive(previewType == Configuration.PreviewGorilla.Steady);

                if (loadedAudio.Count > 0)
                {
                    var type = previewType == Configuration.PreviewGorilla.Silly ? EShirtAudio.SillySpeech : EShirtAudio.SteadySpeech;
                    if (loadedAudio.TryGetValue(type, out AudioClip audio))
                    {
                        standAudio.PlayOneShot(audio, 1f);
                    }
                }
            };

            standRig.OnShirtWorn += delegate
            {
                bool invisibility = standRig.ApplyInvisibility;

                standRig.MainSkin.forceRenderingOff = invisibility;
                standRig.Head.Find("Face").gameObject.SetActive(!invisibility);
                standRig.Body.Find("Chest").gameObject.SetActive(!invisibility);

                standRig.SillyHat.forceRenderingOff = invisibility || standRig.Shirts.Any(shirt => shirt.Template.transform.Find(EShirtComponentType.Head.ToString()));
                standRig.SteadyHat.forceRenderingOff = invisibility || standRig.Shirts.Any(shirt => shirt.Template.transform.Find(EShirtComponentType.Head.ToString()));

                standRig.Objects[standRig.Shirts.Last()].DoIf(a => a.GetComponentInChildren<AudioSource>(), a =>
                {
                    a.GetComponentsInChildren<AudioSource>().Do(src =>
                    {
                        src.playOnAwake = false;
                        src.Stop();
                    });
                });
            };

            SetInfoVisibility += delegate (bool isActive)
            {
                Stand.Display.TextParent.SetActive(!isActive);
                Stand.Display.TextParent.transform.parent.Find("Info Text").gameObject.SetActive(isActive);

                string build_config = "Release";
#if DEBUG
                build_config = "Debug";
#endif

                string player_data = Stand.Display.TextParent.transform.parent.Find("Info Text/PlayerDataTitle").GetComponent<Text>().text
                    .Replace("[SHIRTCOUNT]", Packs.Select((a) => a.Items.Count).Sum().ToString())
                    .Replace("[PACKCOUNT]", Packs.Count.ToString())
                    .Replace("[BUILDCONFIG]", build_config)
                    .Replace("[VERSION]", Constants.Version)
                    .Replace("[PLAYERNAME]", NetworkSystem.Instance.GetMyNickName());

                Stand.Display.TextParent.transform.parent.Find("Info Text/PlayerDataTitle").GetComponent<Text>().text = player_data;

                Stand.Display.UpdateDisplay(Selection, LocalRig.RigHandler.Shirts);
            };

            standRig.SillyHat = standRig.Head.Find("Flower Crown").GetComponent<MeshRenderer>();
            standRig.SteadyHat = standRig.Head.Find("Headphones").GetComponent<MeshRenderer>();
            standRig.SetAppearance(Configuration.PreviewGorillaEntry.Value == Configuration.PreviewGorilla.Silly);
            Stand.Rig = standRig;

            var UITextParent = Stand.Display.TextParent;
            UITextParent.SetActive(false);

            Stand.Display.SetSlots(null);

            var UIButtonParent = Stand.Display.ButtonParent;

            BoxCollider[] UIButtonCollection = Stand.Display.GetComponentsInChildren<BoxCollider>(true);
            UIButtonCollection.Do(btn =>
            {
                if (Enum.TryParse(btn.gameObject.name, out EButtonType type))
                {
                    Button UIButton = btn.gameObject.AddComponent<Button>();
                    UIButton.Type = type;
                    UIButton.OnPress += (GorillaTriggerColliderHandIndicator component) =>
                    {
                        if (loadedAudio.TryGetValue(EShirtAudio.ButtonPress, out AudioClip audio))
                        {
                            var handPlayer = component.isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
                            handPlayer.PlayOneShot(audio, 0.35f);

                            buttons.Find(button => button.ButtonType == UIButton.Type)?.ButtonActivation();
                        }
                    };

                    SetInfoVisibility += delegate (bool isActive)
                    {
                        if (isActive || type == EButtonType.Info)
                        {
                            UIButton.gameObject.SetActive(type == EButtonType.Info);
                            if (type >= EButtonType.Return && type <= EButtonType.TagDecrease)
                                UIButton.transform.parent.gameObject.SetActive(type == EButtonType.Info);
                            return;
                        }

                        bool hasValidSelection = CurrentPack is not null;

                        if (type == EButtonType.Return)
                        {
                            UIButton.gameObject.SetActive(hasValidSelection);
                            return;
                        }

                        bool active = (type >= EButtonType.Return && type <= EButtonType.TagDecrease && hasValidSelection) || (type >= EButtonType.ShirtEquip && type <= EButtonType.ShirtDecrease) || type == EButtonType.Info;
                        if (type >= EButtonType.Return && type <= EButtonType.TagDecrease)
                            UIButton.transform.parent.gameObject.SetActive(active);
                        UIButton.gameObject.SetActive(active);
                    };

                    OnHierarchyChanged += () =>
                    {
                        bool hasValidSelection = CurrentPack is not null;

                        if (type == EButtonType.Return)
                        {
                            UIButton.gameObject.SetActive(hasValidSelection);
                            return;
                        }

                        bool active = (type >= EButtonType.Return && type <= EButtonType.TagDecrease && hasValidSelection) || (type >= EButtonType.ShirtEquip && type <= EButtonType.ShirtDecrease) || type == EButtonType.Info;
                        if (type >= EButtonType.Return && type <= EButtonType.TagDecrease)
                            UIButton.transform.parent.gameObject.SetActive(active);
                        UIButton.gameObject.SetActive(active);
                    };
                }
            });
            UIButtonParent.gameObject.SetActive(false);

            Stand.Object.transform.Find("UI/LoadMenu").gameObject.SetActive(true);
            Stand.Object.transform.Find("UI/MainMenu").gameObject.SetActive(false);

            Stand.OnZoneChange([GTZone.forest]);
        }

        public async Task InitAudio()
        {
            foreach (var name in Enum.GetNames(typeof(EShirtAudio)))
            {
                var audio = await AssetLoader.LoadAsset<AudioClip>(name);
                if (audio)
                {
                    loadedAudio.TryAdd((EShirtAudio)Enum.Parse(typeof(EShirtAudio), name), audio);
                }
            }
        }

        public void CatalogLoadStart()
        {
            CatalogLoadChanged((0, 0));
        }

        public void CatalogLoadChanged((int shirtsLoaded, int shirtsToLoad) tuple)
        {
            if (Stand.Object.transform.Find("UI/LoadMenu/LoadCircle").TryGetComponent(out Image loadCircle))
            {
                loadCircle.fillAmount = Mathf.Clamp01(tuple.shirtsLoaded / (float)tuple.shirtsToLoad);
            }
            if (Stand.Object.transform.Find("UI/LoadMenu/LoadPercentText").TryGetComponent(out Text LoadPercentText))
            {
                LoadPercentText.text = $"{Mathf.RoundToInt(Mathf.Clamp01(tuple.shirtsLoaded / (float)tuple.shirtsToLoad) * 100f)}%";
            }
        }

        public async Task InitCatalog()
        {
            ShirtLoader = new ShirtLoader
            {
                BasePath = Path.GetDirectoryName(GetType().Assembly.Location)
            };

            Packs = await ShirtLoader.GetAllPacks(CatalogLoadStart, CatalogLoadChanged);

            Packs.Sort((x, y) => string.Compare(x.Name, y.Name));
            Packs = [.. Packs.OrderBy(x => x.Name == "Default" ? 0 : (x.Name == "Custom" ? 1 : 2))];

            string[] shirt_names = string.IsNullOrEmpty(Configuration.CurrentShirt.Value) ? null : Configuration.CurrentShirt.Value.Split(Environment.NewLine);
            Logging.Info("Got shirt names");

            List<(IShirtAsset shirt, Pack<IShirtAsset> pack)> shirtsToWear = [];

            foreach (var myPack in Packs)
            {
                if (myPack.Name == "Default") myPack.Shuffle();
                else myPack.Items.Sort((x, y) => string.Compare(x?.Descriptor?.DisplayName ?? "a", y?.Descriptor?.DisplayName ?? "b"));

                foreach (var myShirt in myPack.Items)
                {
                    if (!Shirts.ContainsKey(myShirt.Descriptor.Name))
                    {
                        Shirts.Add(myShirt.Descriptor.Name, myShirt);
                    }

                    if (shirt_names != null && shirt_names.Contains(myShirt.Descriptor.Name) && !LocalRig.RigHandler.Shirts.Contains(myShirt))
                    {
                        Logging.Info("Using shirt from previous session '" + myShirt.Descriptor.DisplayName + "' in pack '" + myPack.Name + "'");
                        shirtsToWear.Add((myShirt, myPack));
                    }
                }
            }

            if (shirtsToWear.Count > 0)
            {
                int shirtVolume = shirtsToWear.Count;
                float soundVolume = 1f / shirtsToWear.Count;

                // SelectedShirt = shirtsToWear[Random.Range(0, shirtVolume)].shirt;
                
                foreach (var (shirt, pack) in shirtsToWear)
                {
                    pack.Selection = pack.Items.IndexOf(shirt);

                    LocalRig.RigHandler.WearShirt(shirt);
                    if (shirt.Descriptor.CustomWearSound)
                    {
                        PlayCustomAudio(LocalRig.PlayerRig, shirt.Descriptor.CustomWearSound, 0.5f * soundVolume);
                    }
                    else
                    {
                        PlayShirtAudio(LocalRig.PlayerRig, EShirtAudio.ShirtWear, 0.5f * soundVolume);
                    }
                }
            }

            UpdateTagOffset();
        }

        public void PlaySound(EShirtAudio audio, float volume = 1f)
        {
            if (loadedAudio.TryGetValue(audio, out var clip))
            {
                Stand.Audio.PlayOneShot(clip, volume);
            }
        }

        public void UpdateWornShirt()
        {
            var shirt = SelectedShirt;
            if (LocalRig.RigHandler.Shirts.Contains(shirt))
            {
                LocalRig.RigHandler.RemoveShirt(shirt);

                if (shirt.Descriptor.CustomRemoveSound)
                {
                    PlayCustomAudio(LocalRig.PlayerRig, shirt.Descriptor.CustomRemoveSound, 0.5f);
                }
                else
                {
                    PlayShirtAudio(LocalRig.PlayerRig, EShirtAudio.ShirtRemove, 0.5f);
                }
            }
            else
            {
                LocalRig.RigHandler.WearShirt(shirt);

                if (shirt.Descriptor.CustomWearSound)
                {
                    PlayCustomAudio(LocalRig.PlayerRig, shirt.Descriptor.CustomWearSound, 0.5f);
                }
                else
                {
                    PlayShirtAudio(LocalRig.PlayerRig, EShirtAudio.ShirtWear, 0.5f);
                }
            }

            Configuration.UpdateGorillaShirt([.. LocalRig.RigHandler.ShirtNames]);

            UpdateProperties();
        }

        public void UpdateTagOffset()
        {
            LocalRig.RigHandler.OffsetNameTag(Configuration.CurrentTagOffset.Value);
            Stand.Rig.OffsetNameTag(Configuration.CurrentTagOffset.Value);

            UpdateProperties();
        }

        private void UpdateProperties()
        {
            string[] shirt_names = [.. LocalRig.RigHandler.ShirtNames];
            int tagOffset = Configuration.CurrentTagOffset.Value;

            NetworkHandler.Instance.SetProperty("Shirts", shirt_names);
            NetworkHandler.Instance.SetProperty("TagOffset", tagOffset);
        }

        public void PlayShirtAudio(VRRig playerRig, EShirtAudio audio, float volume)
        {
            if (playerRig && loadedAudio.TryGetValue(audio, out var clip))
            {
                playerRig.tagSound.PlayOneShot(clip, volume);
            }
        }

        public void PlayCustomAudio(VRRig playerRig, AudioClip clip, float volume)
        {
            if (!playerRig || !clip) return;
            playerRig.tagSound.PlayOneShot(clip, volume);
        }
    }
}