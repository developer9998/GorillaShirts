using BepInEx;
using BepInEx.Bootstrap;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Buttons;
using GorillaShirts.Extensions;
using GorillaShirts.Interfaces;
using GorillaShirts.Locations;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using GorillaShirts.Utilities;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Button = GorillaShirts.Behaviours.UI.Button;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace GorillaShirts.Behaviours
{
    public class Main : MonoBehaviourPunCallbacks
    {
        public static Main Instance;

        public static Dictionary<string, Shirt> TotalInitializedShirts = [];

        public ShirtRig LocalRig;

        public Action<bool> SetInfoVisibility;
        public bool UseInfoPanel;

        public Stand Stand;
        public Camera Camera;

        public readonly List<IStandButton> _standButtons = new List<Type>()
        {
            typeof(ShirtEquip),
            typeof(ShirtIncrease),
            typeof(ShirtDecrease),
            typeof(PackIncrease),
            typeof(PackDecrease),
            typeof(RigToggle),
            typeof(Randomize),
            typeof(TagIncrease),
            typeof(TagDecrease),
            typeof(Information),
            typeof(Capture)
        }.FromTypeCollection<IStandButton>();

        public readonly List<IStandLocation> _standLocations = new List<Type>()
        {
            typeof(Forest),
            typeof(Cave),
            typeof(Canyon),
            typeof(City),
            typeof(Mountain),
            typeof(Clouds),
            typeof(Basement),
            typeof(Beach),
            typeof(Tutorial),
            typeof(Rotating),
            typeof(Metropolis),
            typeof(Arcade),
            typeof(Bayou),
            typeof(VirtualStump)
        }.FromTypeCollection<IStandLocation>();

        public int SelectedPackIndex;
        public List<Pack> ConstructedPacks = [];

        private AssetLoader _assetLoader;

        private Installation _shirtInstaller;

        private List<AudioClip> _audios = [];

        public Shirt SelectedShirt
        {
            get => SelectedPack.PackagedShirts[SelectedPack.CurrentItem];
            set
            {
                foreach (var pack in ConstructedPacks)
                {
                    if (pack.PackagedShirts.Contains(value))
                    {
                        pack.CurrentItem = pack.PackagedShirts.IndexOf(value);
                        SelectedPack = pack;
                        break;
                    }
                }
            }
        }

        public Pack SelectedPack
        {
            get => ConstructedPacks[SelectedPackIndex];
            set => SelectedPackIndex = ConstructedPacks.IndexOf(value);
        }

        public Hashtable CustomProperties;

        private bool IsUpdatingProperties;
        private float PropertyUpdateTime;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            Logging.Info("Main Awake");
        }

        public async void Start()
        {
            var requestVersion = UnityWebRequest.Get(@"https://raw.githubusercontent.com/developer9998/GorillaShirts/main/Version.txt");
            await TaskYieldUtils.Yield(requestVersion);

            if (requestVersion.result != UnityWebRequest.Result.Success)
            {
                Logging.Warning($"GitHub version string resulted with {requestVersion.result}: {requestVersion.downloadHandler.error}");
            }
            else if (requestVersion.downloadHandler.text.Trim() != Constants.Version)
            {
                Logging.Warning($"GitHub version string mismatch, came back with {requestVersion.downloadHandler.text} expecting {Constants.Version}");
                return;
            }

            _assetLoader = new AssetLoader();
            _shirtInstaller = new Installation();

            await InitAll();

            if (ConstructedPacks != null && ConstructedPacks.Count > 0)
            {
                Stand.Display.UpdateDisplay(SelectedShirt, LocalRig.Rig.Shirt, SelectedPack);
                Stand.Display.SetTag(Configuration.CurrentTagOffset.Value);
                Stand.Rig.OffsetNameTag(Configuration.CurrentTagOffset.Value);
                Stand.Rig.WearShirt(SelectedShirt);

                Stand.Object.transform.Find("UI/PrimaryDisplay/Buttons").gameObject.SetActive(true);
                Stand.Object.transform.Find("UI/PrimaryDisplay/Text").gameObject.SetActive(true);
                Stand.Object.transform.Find("UI").GetComponent<Animator>().Play("FadeInFrame");
            }

            if (PhotonNetwork.InRoom)
            {
                foreach (var player in PhotonNetwork.PlayerListOthers)
                {
                    OnPlayerPropertiesUpdate(player, player.CustomProperties);
                }
            }
        }

        public async Task InitAll()
        {
            await InitStand();
            try
            {
                if (IsIncompatibile())
                {
                    Stand.Object.transform.Find("UI").GetComponent<Animator>().Play("IncompFrame");
                    return;
                }
            }
            catch(Exception ex)
            {
                Logging.Warning($"Imcomp threw an exception: {ex}\nproceeding per usual");
            }
            await InitAudio();
            await InitCatalog();
        }

        public async Task InitStand()
        {
            LocalRig = GorillaTagger.Instance.offlineVRRig.gameObject.AddComponent<ShirtRig>();
            LocalRig.Player = PhotonNetwork.LocalPlayer;

            GameObject shirtStand = Instantiate(await _assetLoader.LoadAsset<GameObject>("ShirtStand"));
            shirtStand.name = "Shirt Stand";
            shirtStand.transform.position = _standLocations.First().Location.Item1;
            shirtStand.transform.rotation = Quaternion.Euler(_standLocations.First().Location.Item2);
            AudioSource standAudio = shirtStand.transform.Find("MainSource").GetComponent<AudioSource>();

            ZoneManagement.OnZoneChange += OnZoneChange;

            StandRig standRig = new()
            {
                RigParent = shirtStand.transform.Find("Preview Gorilla")
            };
            standRig.RigParent.Find("Rig").gameObject.AddComponent<Punch>();

            Camera = shirtStand.transform.Find("Camera").GetComponent<Camera>();
            Camera.gameObject.SetActive(false);

            standRig.RigSkin = standRig.RigParent.GetComponentInChildren<SkinnedMeshRenderer>();
            standRig.StandNameTag = standRig.RigParent.GetComponentInChildren<Text>();
            standRig.Head = standRig.RigParent.Find("Rig/body/head");
            standRig.Body = standRig.RigParent.Find("Rig/body");
            standRig.LeftHand = standRig.RigParent.Find("Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L");
            standRig.RightHand = standRig.RigParent.Find("Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R");
            standRig.LeftLower = standRig.LeftHand.parent;
            standRig.RightLower = standRig.RightHand.parent;
            standRig.LeftUpper = standRig.LeftLower.parent;
            standRig.RightUpper = standRig.RightLower.parent;

            // Register the IK
            GorillaIKNonManaged gorillaIk = standRig.RigParent.gameObject.AddComponent<GorillaIKNonManaged>();
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
                shirtStand.transform.Find("UI/PrimaryDisplay/Text/Interaction/Silly Icon").gameObject.SetActive(previewType == Configuration.PreviewGorilla.Silly);
                shirtStand.transform.Find("UI/PrimaryDisplay/Text/Interaction/Steady Icon").gameObject.SetActive(previewType == Configuration.PreviewGorilla.Steady);

                if (_audios.Count > 0)
                {
                    standAudio.clip = previewType == Configuration.PreviewGorilla.Silly ? _audios[3] : _audios[4];
                    standAudio.PlayOneShot(standAudio.clip, 1f);
                }
            };

            standRig.OnShirtWorn += delegate
            {
                bool invisibility = standRig.Shirt.Invisibility;

                standRig.RigSkin.forceRenderingOff = invisibility;
                standRig.Head.Find("Face").gameObject.SetActive(!invisibility);
                standRig.Body.Find("Chest").gameObject.SetActive(!invisibility);

                standRig.SillyHat.forceRenderingOff = invisibility || standRig.Shirt.SectorList.Any((a) => a.Type == SectorType.Head);
                standRig.SteadyHat.forceRenderingOff = invisibility || standRig.Shirt.SectorList.Any((a) => a.Type == SectorType.Head);

                standRig.Objects[standRig.Shirt].DoIf(a => a.GetComponentInChildren<AudioSource>(), a =>
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
                shirtStand.transform.Find("UI/PrimaryDisplay/Text").gameObject.SetActive(!isActive);
                shirtStand.transform.Find("UI/PrimaryDisplay/Info Text").gameObject.SetActive(isActive);

                StringBuilder stringBuilder = new();
                stringBuilder.Append("Shirts: ").Append(ConstructedPacks.Select((a) => a.PackagedShirts.Count).Sum()).Append(" | Packs: ").Append(ConstructedPacks.Count);
                shirtStand.transform.Find("UI/PrimaryDisplay/Info Text/Left Body").GetComponent<Text>().text = stringBuilder.ToString();

                stringBuilder = new StringBuilder();
                stringBuilder.Append("Build: ");
#if DEBUG
                stringBuilder.Append("Debug");
#else
                stringBuilder.Append("Release");
#endif

                stringBuilder.Append(" | Version: ").Append(Constants.Version);
                shirtStand.transform.Find("UI/PrimaryDisplay/Info Text/Right Body").GetComponent<Text>().text = stringBuilder.ToString();
            };

            standRig.SillyHat = standRig.Head.Find("Flower Crown").GetComponent<MeshRenderer>();
            standRig.SteadyHat = standRig.Head.Find("Headphones").GetComponent<MeshRenderer>();
            standRig.SetAppearance(Configuration.PreviewGorillaEntry.Value == Configuration.PreviewGorilla.Silly);

            Transform UITextParent = shirtStand.transform.Find("UI/PrimaryDisplay/Text");
            ShirtDisplay standDisplay = new()
            {
                Main = UITextParent.Find("Main").GetComponent<Text>(),
                Body = UITextParent.Find("Body").GetComponent<Text>(),
                Version = UITextParent.Find("Version").GetComponent<Text>(),
                Equip = UITextParent.Find("Interaction/Equip").GetComponent<Text>(),
                Pack = UITextParent.Find("Interaction/Pack").GetComponent<Text>(),
                Tag = UITextParent.Find("Interaction/Nametag").GetComponent<Text>(),
                SlotParent = UITextParent.Find("Interaction/Slots").gameObject
            };
            UITextParent.gameObject.SetActive(false);

            for (int x = 0; x < standDisplay.SlotParent.transform.childCount; x++)
            {
                Transform slotItem = standDisplay.SlotParent.transform.GetChild(x);
                standDisplay.SlotItems.Add(slotItem.gameObject);
            }

            standDisplay.SetSlots(null);

            Transform UIButtonParent = shirtStand.transform.Find("UI/PrimaryDisplay/Buttons");
            BoxCollider[] UIButtonCollection = UIButtonParent.GetComponentsInChildren<BoxCollider>();
            UIButtonCollection.Do(btn =>
            {
                Button UIButton = btn.gameObject.AddComponent<Button>();
                UIButton.Type = Button.GetButtonType(btn.name);
                UIButton.OnPress += delegate (GorillaTriggerColliderHandIndicator component)
                {
                    if (!_audios.Any()) return;

                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(GorillaLocomotion.Player.Instance.materialData.Count - 1, component.isLeftHand, 0.028f);

                    if (!ConstructedPacks.Any()) return;
                    _standButtons.Find(button => button.Type == UIButton.Type)?.Function?.Invoke(this);
                };

                SetInfoVisibility += delegate (bool isActive)
                {
                    ButtonType type = UIButton.Type;
                    UIButton.gameObject.SetActive(!isActive || type == ButtonType.Info);
                };
            });
            UIButtonParent.gameObject.SetActive(false);

            Stand = new Stand()
            {
                Display = standDisplay,
                Audio = standAudio,
                Rig = standRig,
                Object = shirtStand
            };
        }

        private void OnZoneChange(ZoneData[] zones)
        {
            IEnumerable<GTZone> activeZones = zones.Where(zone => zone.active).Select(zone => zone.zone);
            OnZoneChange(activeZones.ToArray());
        }

        public void OnZoneChange(GTZone[] zones)
        {
            Logging.Info($"Zone changed: {string.Join(", ", zones)}");

            foreach (GTZone currentZone in zones)
            {
                IStandLocation currentLocation = _standLocations.FirstOrDefault(zone => zone.IsInZone(currentZone));

                if (currentLocation != null)
                {
                    Logging.Info($"We are in {currentLocation.GetType().Name} ({currentZone} is active)");

                    Tuple<Vector3, Vector3> locationData = currentLocation.Location;
                    MoveStand(locationData.Item1, locationData.Item2);
                    return;
                }
            }

            Logging.Error($"No stand location exists for zones {string.Join(", ", zones)}, hiding shirt stand");
            Stand.Object.SetActive(false);
        }

        public void MoveStand(Transform transform) => MoveStand(transform.position, transform.eulerAngles);

        public void MoveStand(Vector3 position, Vector3 direction)
        {
            Stand.Object.transform.position = position;
            Stand.Object.transform.eulerAngles = direction;
            Stand.Object.SetActive(true);
        }

        public async Task InitAudio()
        {
            _audios =
            [
                await _assetLoader.LoadAsset<AudioClip>("Wear"),
                await _assetLoader.LoadAsset<AudioClip>("Remove"),
                await _assetLoader.LoadAsset<AudioClip>("Button"),
                await _assetLoader.LoadAsset<AudioClip>("SillyTXT"),
                await _assetLoader.LoadAsset<AudioClip>("SteadyTXT"),
                await _assetLoader.LoadAsset<AudioClip>("Randomize"),
                await _assetLoader.LoadAsset<AudioClip>("Error"),
                await _assetLoader.LoadAsset<AudioClip>("Shutter"),
                await _assetLoader.LoadAsset<AudioClip>("PackOpen"),
                await _assetLoader.LoadAsset<AudioClip>("PackClose"),
            ];

            GorillaLocomotion.Player.Instance.materialData.Add(new GorillaLocomotion.Player.MaterialData()
            {
                overrideAudio = true,
                audio = _audios[2],
                matName = "gorillashirtbuttonpress"
            });
        }

        public void PlayShirtAudio(VRRig playerRig, int index, float volume)
        {
            if (playerRig == null || index >= _audios.Count) return;
            playerRig.tagSound.PlayOneShot(_audios[index], volume);
        }

        public void PlayCustomAudio(VRRig playerRig, AudioClip clip, float volume)
        {
            if (!playerRig || !clip) return;
            playerRig.tagSound.PlayOneShot(clip, volume);
        }

        public bool IsIncompatibile()
        {
            foreach (var pluginInfo in Chainloader.PluginInfos.Values)
            {
                Assembly pluginAssembly = pluginInfo.Instance.GetType().Assembly;
                var pluginTypes = pluginAssembly.GetTypes();
                if (pluginInfo.Metadata.GUID == "com.nachoengine.playermodel" || pluginInfo.Metadata.GUID == "com.wryser.gorillatag.customcosmetics" || pluginTypes.Any(type => type.Name.Contains("WristMenu") || type.Name.Contains("MenuPatch") || type.Name.Contains("Cosmetx")))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task InitCatalog()
        {
            ConstructedPacks = await _shirtInstaller.FindShirtsFromDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            if (ConstructedPacks == null || ConstructedPacks.Count == 0)
            {
                Logging.Warning("No packs found");
                Destroy(Stand.Object);
                return;
            }

            // Sort the packs based on their name, and prioritize the "Default" pack
            ConstructedPacks.Sort((x, y) => string.Compare(x.Name, y.Name));
            ConstructedPacks = [.. ConstructedPacks.OrderBy(a => a.Name == "Default" ? 0 : 1)];

            foreach (var myPack in ConstructedPacks)
            {
                if (myPack.DisplayName == "Default") myPack.Randomize();
                else myPack.PackagedShirts.Sort((x, y) => string.Compare(x.Name, y.Name));

                foreach (var myShirt in myPack.PackagedShirts)
                {
                    TotalInitializedShirts.TryAdd(myShirt.Name, myShirt);
                    if (myShirt.Name == Configuration.CurrentShirt.Value && LocalRig.Rig.Shirt != myShirt)
                    {
                        Logging.Info("Using shirt from previous session '" + myShirt.DisplayName + "' in pack '" + myPack.DisplayName + "'");
                        SelectedShirt = myShirt;
                        UpdatePlayerHash(true);
                    }
                }
            }
        }

        public void PlaySound(ShirtAudio audio, float volume = 1f)
        {
            AudioSource mainSource = Stand.Object.transform.Find("MainSource").GetComponent<AudioSource>();
            mainSource.clip = _audios[(int)audio];
            mainSource.PlayOneShot(mainSource.clip, volume);
        }

        public void SetPackInfo(Pack myPack, Shirt myShirt)
        {
            SelectedPackIndex = ConstructedPacks.IndexOf(myPack);
            myPack.CurrentItem = myPack.PackagedShirts.IndexOf(myShirt);
        }

        public IEnumerator Capture(Camera camera)
        {
            string directory = Path.Combine(Paths.BepInExRootPath, "GorillaShirts Captures");
            string file = Path.Combine(directory, string.Format("{0:yy-MM-dd-HH-mm-ss-ff}.png", DateTime.Now));

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            yield return new WaitForEndOfFrame();

            RenderTexture renderTexture = camera.targetTexture;

            RenderTexture.active = renderTexture;

            int width = renderTexture.width;
            int height = renderTexture.height;
            RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
            Texture2D tex = new(width, height, TextureFormat.RGB24, false);

            RenderTexture.active = renderTex;
            camera.targetTexture = renderTex;

            camera.Render();

            camera.targetTexture = renderTexture;

            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            RenderTexture.active = null;

            RenderTexture.ReleaseTemporary(renderTex);
            File.WriteAllBytes(file, tex.EncodeToPNG());

            yield break;
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

        public void UpdatePlayerHash(bool useShirtSelection = false)
        {
            Shirt currentShirt = useShirtSelection ? SelectedShirt : LocalRig.Rig.Shirt;

            if (useShirtSelection && currentShirt != null && currentShirt == LocalRig.Rig.Shirt)
            {
                currentShirt = null;
            }

            string shirtName = currentShirt?.Name;

            CustomProperties = new()
            {
                {
                    Constants.HashKey,
                    new object[] { shirtName, Configuration.CurrentTagOffset.Value }
                }
            };

            IsUpdatingProperties = true;

            CheckHash(PhotonNetwork.LocalPlayer, CustomProperties);
        }

        public void AddShirtRig(VRRig playerRig)
        {
            if (playerRig == null)
            {
                Logging.Error("Attempted to add a ShirtRig to a null VRRig");
                return;
            }

            Player player;

            if (playerRig.isOfflineVRRig)
            {
                player = PhotonNetwork.LocalPlayer;
            }
            else
            {
                player = PhotonNetwork.CurrentRoom.GetPlayer(playerRig.Creator.ActorNumber);
            }

            if (player == null)
            {
                Logging.Error("Attempted to add a ShirtRig to a VRRig with a null player");
                return;
            }

            Logging.Info($"Adding ShirtRig to player {player.NickName}");
            GetShirtRig(player, playerRig);
            Logging.Info("Added");
        }

        public void RemoveShirtRig(VRRig playerRig)
        {
            ShirtRig shirtRig = playerRig.GetComponent<ShirtRig>();

            if (shirtRig == null)
            {
                Logging.Error("Attempted to remove a null ShirtRig");
                return;
            }

            Player player = shirtRig.Player;

            if (player == null)
            {
                Logging.Error("Attempted to remove a ShirtRig with a null player");
                return;
            }

            if (player.IsLocal)
            {
                Logging.Error("Attempted to remove a ShirtRig for the local player");
                return;
            }

            Logging.Info($"Removing ShirtRig from player {player.NickName}");

            Destroy(shirtRig);

            Logging.Info("Removed");
        }

        public ShirtRig GetShirtRig(Player player, VRRig playerRig = null)
        {
            if (!playerRig)
            {
                playerRig = RigUtils.GetPlayerRig(player);
            }

            if (!playerRig.TryGetComponent(out ShirtRig shirtRig))
            {
                shirtRig = playerRig.AddComponent<ShirtRig>();
                shirtRig.Player = player;
            }

            return shirtRig;
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

            if (!targetPlayer.IsLocal)
            {
                CheckHash(targetPlayer, changedProps);
            }
        }

        public void CheckHash(Player targetPlayer, Hashtable changedProps)
        {
            VRRig playerRig;

            if (targetPlayer.IsLocal)
            {
                playerRig = GorillaTagger.Instance.offlineVRRig;
            }
            else
            {
                playerRig = RigUtils.GetVRRig(targetPlayer);
            }

            if (playerRig == null)
            {
                Logging.Error($"{targetPlayer.NickName} has a null VRRig");
                return;
            }

            ShirtRig shirtRig = GetShirtRig(targetPlayer);

            try
            {
                if (changedProps.TryGetValue(Constants.HashKey, out object value) && value is object[] gsHash)
                {
                    Logging.Info($"{targetPlayer.NickName} has updated hash to {string.Join(", ", gsHash)}");

                    int tagOffset = (int)gsHash[1];

                    if (targetPlayer.IsLocal)
                    {
                        CustomProperties = changedProps;
                        LocalRig.Rig.NameTagOffset = Configuration.CurrentTagOffset.Value;
                        Stand.Display.SetTag(Configuration.CurrentTagOffset.Value);
                    }
                    else
                    {
                        Logging.Info($"{targetPlayer.NickName} has a name tag offset of {tagOffset}");
                        shirtRig.Rig.OffsetNameTag(tagOffset);
                    }

                    string wornGorillaShirt = (string)gsHash[0];

                    if (wornGorillaShirt != null && TotalInitializedShirts.ContainsKey(wornGorillaShirt))
                    {
                        Shirt newShirt = TotalInitializedShirts[wornGorillaShirt];

                        Logging.Info($"{targetPlayer.NickName} is wearing shirt {newShirt.DisplayName}");

                        if (targetPlayer.IsLocal)
                        {
                            Stand.Display.SetEquipped(newShirt, LocalRig.Rig.Shirt);
                            Configuration.UpdateGorillaShirt(wornGorillaShirt);
                        }

                        shirtRig.Rig.WearShirt(newShirt, out Shirt oldShirt);

                        if (oldShirt == newShirt)
                        {
                            LocalRig.Rig.MoveNameTag();
                            return; // check for if a sound should be made
                        }

                        if (newShirt.Wear)
                        {
                            // play a custom shirt wearing audio
                            PlayCustomAudio(playerRig, newShirt.Wear, 0.5f);
                        }
                        else
                        {
                            // play the default shirt wearing audio
                            PlayShirtAudio(playerRig, 0, 0.5f);
                        }

                        return;
                    }

                    Shirt currentShirt = shirtRig.Rig.Shirt;

                    if (currentShirt != null)
                    {
                        Logging.Info($"{targetPlayer.NickName} is removing shirt {currentShirt.DisplayName}");

                        shirtRig.Rig.RemoveShirt();
                        shirtRig.Rig.MoveNameTag();

                        if (targetPlayer.IsLocal)
                        {
                            Stand.Display.SetEquipped(null, LocalRig.Rig.Shirt);
                            Configuration.UpdateGorillaShirt(null);
                        }

                        if (shirtRig.Rig.Shirt == currentShirt) return; // check for if a sound should be made

                        if (shirtRig.Rig.Shirt != null && currentShirt.Remove)
                        {
                            // play a custom shirt removal audio
                            PlayCustomAudio(playerRig, currentShirt.Remove, 0.5f);
                        }
                        else
                        {
                            // play the default shirt removal audio
                            PlayShirtAudio(playerRig, 1, 0.5f);
                        }

                        return;
                    }

                    if (wornGorillaShirt != null && !string.IsNullOrEmpty(wornGorillaShirt))
                    {
                        Logging.Info($"{targetPlayer.NickName} is wearing shirt {wornGorillaShirt} (missing)");

                        // play the shirt missing audio
                        PlayShirtAudio(playerRig, 6, 1f);

                        return;
                    }

                    Logging.Warning($"This should not happen ({targetPlayer.NickName}, {wornGorillaShirt})");
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Failed to handle custom props from player {targetPlayer.NickName}: {ex}");
            }
        }
    }
}