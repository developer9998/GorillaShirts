using BepInEx;
using BepInEx.Bootstrap;
using GorillaLocomotion;
using GorillaShirts.Buttons;
using GorillaShirts.Extensions;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Locations;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Button = GorillaShirts.Interaction.Button;

namespace GorillaShirts.Behaviours
{
    public class Main : MonoBehaviourPunCallbacks
    {
        public static Main Instance;

        public static Dictionary<string, Shirt> TotalInitializedShirts = [];    

        public Networking Networking;

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
            typeof(Arcade)
        }.FromTypeCollection<IStandLocation>();

        public int SelectedPackIndex;
        public List<Pack> ConstructedPacks = [];

        private AssetLoader _assetLoader;

        private Installation _shirtInstaller;

        private List<AudioClip> _audios = [];

        public Shirt SelectedShirt => SelectedPack.PackagedShirts[SelectedPack.CurrentItem];
        public Pack SelectedPack => ConstructedPacks[SelectedPackIndex];

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
            Networking = gameObject.AddComponent<Networking>();

            _assetLoader = new AssetLoader();
            _shirtInstaller = new Installation();

            await InitAll();

            if (ConstructedPacks != null && ConstructedPacks.Count > 0)
            {
                Stand.Display.UpdateDisplay(SelectedShirt, LocalRig.Rig.Shirt, SelectedPack);
                Stand.Display.SetTag(Configuration.CurrentTagOffset.Value);
                Stand.Rig.SetTagOffset(Configuration.CurrentTagOffset.Value);
                Stand.Rig.WearShirt(SelectedShirt);

                Stand.Object.transform.Find("UI/PrimaryDisplay/Buttons").gameObject.SetActive(true);
                Stand.Object.transform.Find("UI/PrimaryDisplay/Text").gameObject.SetActive(true);
                Stand.Object.transform.Find("UI").GetComponent<Animator>().Play("FadeInFrame");
            }

            if (PhotonNetwork.InRoom)
            {
                foreach (var player in PhotonNetwork.PlayerListOthers)
                {
                    Networking.Instance.OnPlayerPropertiesUpdate(player, player.CustomProperties);
                    //Events.CustomPropUpdate?.Invoke(player, player.CustomProperties);
                }
            }
        }

        public async Task InitAll()
        {
            await InitStand();
            if (IsIncompatibile())
            {
                Stand.Object.transform.Find("UI").GetComponent<Animator>().Play("IncompFrame");
                return;
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

            ZoneManagement.OnZoneChange += (ZoneData[] zones) =>
            {
                IEnumerable<GTZone> changedZones = zones.Select(zone => zone.zone);

                foreach (GTZone currentZone in changedZones)
                {
                    IStandLocation currentLocation = _standLocations.FirstOrDefault(zone => zone.IsInZone(currentZone));
                    if (currentLocation != null)
                    {
                        try
                        {
                            Tuple<Vector3, Vector3> locationData = currentLocation.Location;
                            shirtStand.transform.position = locationData.Item1;
                            shirtStand.transform.rotation = Quaternion.Euler(locationData.Item2);
                            shirtStand.SetActive(true);
                        }
                        catch
                        {
                            Logging.Error($"No stand location exists for zones {string.Join(", ", changedZones)}, hiding shirt stand");
                            shirtStand.SetActive(false);
                        }
                        break;
                    }
                }
            };

            StandRig standRig = new()
            {
                Toggle = false,
                RigParent = shirtStand.transform.Find("Preview Gorilla")
            };
            standRig.RigParent.Find("Rig").gameObject.AddComponent<Punch>();

            Camera = shirtStand.transform.Find("Camera").GetComponent<Camera>();
            Camera.gameObject.SetActive(false);

            standRig.RigSkin = standRig.RigParent.GetComponentInChildren<SkinnedMeshRenderer>();
            standRig.Nametag = standRig.RigParent.GetComponentInChildren<Text>();
            standRig.Head = standRig.RigParent.Find("Rig/body/head");
            standRig.Body = standRig.RigParent.Find("Rig/body");
            standRig.LeftHand = standRig.RigParent.Find("Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L");
            standRig.RightHand = standRig.RigParent.Find("Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R");
            standRig.LeftLower = standRig.LeftHand.parent;
            standRig.RightLower = standRig.RightHand.parent;
            standRig.LeftUpper = standRig.LeftLower.parent;
            standRig.RightUpper = standRig.RightLower.parent;

            // Register the IK
            GorillaIK gorillaIk = standRig.RigParent.gameObject.AddComponent<GorillaIK>();
            gorillaIk.enabled = true;
            gorillaIk.targetLeft = shirtStand.transform.Find("Preview Gorilla/Rig/LeftTarget");
            gorillaIk.targetRight = shirtStand.transform.Find("Preview Gorilla/Rig/RightTarget");
            gorillaIk.targetHead = standRig.Head;
            gorillaIk.headBone = standRig.Head;
            gorillaIk.leftUpperArm = standRig.LeftUpper;
            gorillaIk.leftLowerArm = standRig.LeftLower;
            gorillaIk.leftHand = standRig.LeftHand;
            gorillaIk.rightUpperArm = standRig.RightUpper;
            gorillaIk.rightLowerArm = standRig.RightLower;
            gorillaIk.rightHand = standRig.RightHand;

            gorillaIk.dU = (gorillaIk.leftUpperArm.position - gorillaIk.leftLowerArm.position).magnitude;
            gorillaIk.dL = (gorillaIk.leftLowerArm.position - gorillaIk.leftHand.position).magnitude;
            gorillaIk.dMax = gorillaIk.dU + gorillaIk.dL - gorillaIk.eps;
            gorillaIk.initialUpperLeft = gorillaIk.leftUpperArm.localRotation;
            gorillaIk.initialLowerLeft = gorillaIk.leftLowerArm.localRotation;
            gorillaIk.initialUpperRight = gorillaIk.rightUpperArm.localRotation;
            gorillaIk.initialLowerRight = gorillaIk.rightLowerArm.localRotation;

            // De-register and Re-register the IK
            gorillaIk.enabled = false;
            gorillaIk.enabled = true;

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

                standRig.CachedObjects[standRig.Shirt].DoIf(a => a.GetComponentInChildren<AudioSource>(), a =>
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

                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(Player.Instance.materialData.Count - 1, component.isLeftHand, 0.028f);

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

            Player.Instance.materialData.Add(new Player.MaterialData()
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
                if (pluginInfo.Metadata.GUID == "com.nachoengine.playermodel" || pluginTypes.Any(type => type.Name.Contains("WristMenu") || type.Name.Contains("MenuPatch") || type.Name.Contains("Cosmetx")))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task InitCatalog()
        {
            ConstructedPacks = await _shirtInstaller.FindShirtsFromDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

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
                        SetPackInfo(myPack, myShirt);
                        SetShirt(myShirt);
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

        public void SetShirt(Shirt newShirt)
        {
            if (newShirt != null)
            {
                LocalRig.Rig.WearShirt(newShirt, out Shirt oldShirt);
                LocalRig.Rig.SetTagOffset(Configuration.CurrentTagOffset.Value);

                if (newShirt != null)
                {
                    if (newShirt.Wear) PlayCustomAudio(LocalRig.GetComponent<VRRig>(), newShirt.Wear, 0.3f);
                    else PlayShirtAudio(LocalRig.GetComponent<VRRig>(), 0, 0.4f);
                }
                else if (oldShirt != null)
                {
                    if (oldShirt.Remove) PlayCustomAudio(LocalRig.GetComponent<VRRig>(), oldShirt.Remove, 0.3f);
                    else PlayShirtAudio(LocalRig.GetComponent<VRRig>(), 1, 0.4f);
                }

                // Networking
                Networking.UpdateProperties(Networking.GenerateHashtable(LocalRig.Rig.Shirt, Configuration.CurrentTagOffset.Value));

                // Configuration
                Configuration.UpdateGorillaShirt(newShirt != null ? newShirt.Name : "None");
            }
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
    }
}