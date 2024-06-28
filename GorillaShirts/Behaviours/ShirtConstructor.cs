using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaShirts.Buttons;
using GorillaShirts.Extensions;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Locations;
using GorillaShirts.Models;
using GorillaShirts.Patches;
using GorillaShirts.Tools;
using GorillaShirts.Utilities;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Button = GorillaShirts.Interaction.Button;

namespace GorillaShirts.Behaviours
{
    public class ShirtConstructor : MonoBehaviourPunCallbacks
    {
        private bool _initalized;

        public ConfigFile ConfigFile;
        public Configuration Config;

        public Networking Networking;

        public PhysicalRig LocalRig;

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
            typeof(Metropolis)
        }.FromTypeCollection<IStandLocation>();

        public int SelectedPackIndex;
        public List<Pack> ConstructedPacks = [];

        private AssetLoader _assetLoader;

        private Installation _shirtInstaller;

        private List<AudioClip> _audios = [];

        public Shirt SelectedShirt => SelectedPack.PackagedShirts[SelectedPack.CurrentItem];
        public Pack SelectedPack => ConstructedPacks[SelectedPackIndex];

        public async void Start()
        {
            if (_initalized) return;
            _initalized = true;

            Networking = gameObject.AddComponent<Networking>();

            _assetLoader = new AssetLoader();
            _shirtInstaller = new Installation();
            Config = new Configuration(ConfigFile);

            // Creates the shirt stands used for interaction with the mod
            #region Stand Initialization

            LocalRig = GorillaTagger.Instance.offlineVRRig.gameObject.AddComponent<PhysicalRig>();

            GameObject shirtStand = Instantiate(await _assetLoader.LoadAsset<GameObject>("ShirtStand"));
            shirtStand.name = "Shirt Stand";
            shirtStand.transform.position = _standLocations.First().Location.Item1;
            shirtStand.transform.rotation = Quaternion.Euler(_standLocations.First().Location.Item2);
            AudioSource standAudio = shirtStand.transform.Find("MainSource").GetComponent<AudioSource>();

            ZonePatches.OnMapUpdate += delegate (GTZone[] zones)
            {
                foreach (GTZone testZone in zones)
                {
                    IStandLocation testLocation = _standLocations.FirstOrDefault(zone => zone.Zone == testZone);
                    if (testLocation != null)
                    {
                        try
                        {
                            Tuple<Vector3, Vector3> locationData = testLocation.Location;
                            shirtStand.transform.position = locationData.Item1;
                            shirtStand.transform.rotation = Quaternion.Euler(locationData.Item2);
                        }
                        catch
                        {
                            // TODO: log error
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

            standRig.OnAppearanceChange += delegate (Configuration.PreviewTypes previewType)
            {
                shirtStand.transform.Find("UI/PrimaryDisplay/Text/Interaction/Silly Icon").gameObject.SetActive(previewType == Configuration.PreviewTypes.Silly);
                shirtStand.transform.Find("UI/PrimaryDisplay/Text/Interaction/Steady Icon").gameObject.SetActive(previewType == Configuration.PreviewTypes.Steady);

                if (_audios.Count > 0)
                {
                    standAudio.clip = previewType == Configuration.PreviewTypes.Silly ? _audios[3] : _audios[4];
                    standAudio.PlayOneShot(standAudio.clip, 1f);
                }
            };

            standRig.OnShirtWorn += delegate
            {
                bool invisibility = standRig.CurrentShirt.Invisibility;

                standRig.RigSkin.forceRenderingOff = invisibility;
                standRig.Head.Find("Face").gameObject.SetActive(!invisibility);
                standRig.Body.Find("Chest").gameObject.SetActive(!invisibility);

                standRig.SillyHat.forceRenderingOff = invisibility || standRig.CurrentShirt.SectorList.Any((a) => a.Type == SectorType.Head);
                standRig.SteadyHat.forceRenderingOff = invisibility || standRig.CurrentShirt.SectorList.Any((a) => a.Type == SectorType.Head);

                standRig.CachedObjects[standRig.CurrentShirt].DoIf(a => a.GetComponentInChildren<AudioSource>(), a =>
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
            standRig.SetAppearance(Config.CurrentPreview.Value == Configuration.PreviewTypes.Silly);

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

                    /* 
                    if (PhotonNetwork.InRoom && GorillaTagger.Instance.myVRRig != null)
                    {
                        GorillaTagger.Instance.myVRRig.RPC("PlayHandTap", (RpcTarget)1, new object[3]
                        {
                            Player.Instance.materialData.Count - 1,
                            component.isLeftHand,
                            0.1f
                        });
                    }
                    */

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

            #endregion

            // Prepares much of the audio-related functions
            #region Audio Initialization

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

            Events.PlayShirtAudio += delegate (VRRig targetRig, int index, float volume)
            {
                if (targetRig == null || index > _audios.Count - 1) return;
                targetRig.tagSound.PlayOneShot(_audios[index], volume);
            };

            Events.PlayCustomAudio += delegate (VRRig targetRig, AudioClip clip, float volume)
            {
                if (!targetRig || !clip) return;
                targetRig.tagSound.PlayOneShot(clip, volume);
            };

            Player.Instance.materialData.Add(new Player.MaterialData()
            {
                overrideAudio = true,
                audio = _audios[2],
                matName = "gorillashirtbuttonpress"
            });

            #endregion

            // Checks chainloader for any incompatible mods
            #region Incompatibility Check

            foreach (var pluginInfo in Chainloader.PluginInfos.Values)
            {
                Assembly pluginAssembly = pluginInfo.Instance.GetType().Assembly;
                var pluginTypes = pluginAssembly.GetTypes();
                if (pluginInfo.Metadata.GUID == "com.nachoengine.playermodel" || pluginTypes.Any(type => type.Name.Contains("WristMenu") || type.Name.Contains("MenuPatch") || type.Name.Contains("Cosmetx")))
                {
                    shirtStand.transform.Find("UI").GetComponent<Animator>().Play("IncompFrame");
                    return;
                }
            }

            #endregion

            // Loads a set of packs from our plugins directory and attempts to wear our shirt from a previous session
            #region Pack Initialization

            ConstructedPacks = await _shirtInstaller.FindShirtsFromDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            // Sort the packs based on their name, and prioritize the "Default" pack
            ConstructedPacks.Sort((x, y) => string.Compare(x.Name, y.Name));
            ConstructedPacks = ConstructedPacks.OrderBy(a => a.Name == "Default" ? 0 : 1).ToList();

            foreach (var myPack in ConstructedPacks)
            {
                if (myPack.DisplayName == "Default") myPack.Randomize();
                else myPack.PackagedShirts.Sort((x, y) => string.Compare(x.Name, y.Name));

                foreach (var myShirt in myPack.PackagedShirts)
                {
                    ShirtUtils.ShirtDict.TryAdd(myShirt.Name, myShirt);
                    if (myShirt.Name == Config.CurrentShirt.Value && LocalRig.Rig.CurrentShirt != myShirt)
                    {
                        Logging.Info("Using shirt from previous session '" + myShirt.DisplayName + "' in pack '" + myPack.DisplayName + "'");
                        SetPackInfo(myPack, myShirt);
                        SetShirt(myShirt);
                    }
                }
            }

            WardrobePatches.CosmeticUpdated += delegate (CosmeticsController.CosmeticCategory category)
            {
                bool condition = LocalRig.Rig.CurrentShirt != null && Config.RemoveBaseItem.Value && (LocalRig.Rig.CurrentShirt.SectorList.Any(a => a.Type == SectorType.Head) && category == CosmeticsController.CosmeticCategory.Hat || category == CosmeticsController.CosmeticCategory.Badge);
                if (condition)
                {
                    SetShirt(LocalRig.Rig.CurrentShirt);
                    Stand.Display.SetEquipped(Stand.Rig.CurrentShirt, LocalRig.Rig.CurrentShirt);
                }
            };

            #endregion

            if (ConstructedPacks != null && ConstructedPacks.Count > 0)
            {
                standRig.SetTagOffset(Config.CurrentTagOffset.Value);

                // Adjust the stand to display our currently worn shirt
                Stand.Display.UpdateDisplay(SelectedShirt, LocalRig.Rig.CurrentShirt, SelectedPack);
                Stand.Display.SetTag(Config.CurrentTagOffset.Value);
                Stand.Rig.Wear(SelectedShirt);

                UIButtonParent.gameObject.SetActive(true);
                UITextParent.gameObject.SetActive(true);
                shirtStand.transform.Find("UI").GetComponent<Animator>().Play("FadeInFrame");
            }

            try
            {
                if (PhotonNetwork.InRoom)
                {
                    foreach(var player in PhotonNetwork.PlayerListOthers)
                    {
                        Events.CustomPropUpdate?.Invoke(player, player.CustomProperties);
                    }
                }
            }
            catch
            {

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

        public void SetShirt(Shirt myShirt)
        {
            if (myShirt != null)
            {
                LocalRig.Rig.Wear(myShirt, out Shirt preShirt, out Shirt postShirt);
                LocalRig.Rig.SetTagOffset(postShirt != null ? Config.CurrentTagOffset.Value : 0);

                // Cosmetics
                if (postShirt != null && Config.RemoveBaseItem.Value)
                {
                    ShirtUtils.RemoveItem(CosmeticsController.CosmeticCategory.Badge, CosmeticsController.CosmeticSlots.Badge);
                    if (postShirt.SectorList.Any(a => a.Type == SectorType.Head)) ShirtUtils.RemoveItem(CosmeticsController.CosmeticCategory.Hat, CosmeticsController.CosmeticSlots.Hat);
                }

                if (postShirt != null)
                {
                    if (postShirt.Wear) Events.PlayCustomAudio?.Invoke(LocalRig.GetComponent<VRRig>(), postShirt.Wear, 0.3f);
                    else Events.PlayShirtAudio?.Invoke(LocalRig.GetComponent<VRRig>(), 0, 0.4f);
                }
                else if (preShirt != null)
                {
                    if (preShirt.Remove) Events.PlayCustomAudio?.Invoke(LocalRig.GetComponent<VRRig>(), preShirt.Remove, 0.3f);
                    else Events.PlayShirtAudio?.Invoke(LocalRig.GetComponent<VRRig>(), 1, 0.4f);
                }

                // Networking
                Networking.UpdateProperties(Networking.GenerateHashtable(LocalRig.Rig.CurrentShirt, Config.CurrentTagOffset.Value));

                // Configuration
                Config.SetCurrentShirt(postShirt != null ? myShirt.Name : "None");
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