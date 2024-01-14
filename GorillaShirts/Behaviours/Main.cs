using BepInEx;
using BepInEx.Bootstrap;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaShirts.Behaviours.Data;
using GorillaShirts.Behaviours.Interaction;
using GorillaShirts.Behaviours.Models;
using GorillaShirts.Behaviours.Tools;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Patches;
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
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Button = GorillaShirts.Behaviours.Interaction.Button;

namespace GorillaShirts.Behaviours
{
    public class Main : MonoBehaviourPunCallbacks, IInitializable
    {
        private bool _initalized;

        private Networking _networking;

        private AssetLoader _assetLoader;

        private Installation _shirtInstaller;
        private Configuration _config;

        private event Action<bool> OnInfoMenuChanged;
        private bool _advancedVisible;

        private Camera _currentCamera;

        private List<AudioClip> _shirtAudioList = new();
        private readonly Dictionary<GTZone, Vector3[]> _standLocationData = new()
        {
            {
                GTZone.forest,
                new Vector3[] {
                    new(-67.6651f, 12.07f, -80.438f),
                    new(0f, 171.1801f, 0f)
                }
            },
            {
                GTZone.cave,
                new Vector3[] {
                    new(-60.525f, -11.7064f, -41.7656f),
                    new(0f, 267.9607f, 0f)
                }
            },
            {
                GTZone.canyon,
                new Vector3[] {
                    new(-100.246f, 17.9809f, -169.374f),
                    new(0f, 297.5797f, 0f)
                }
            },
            {
                GTZone.city,
                new Vector3[] {
                    new(-56.8213f, 17.0026f, -120.0597f),
                    new(0f, 316.1757f, 0f)
                }
            },
            {
                GTZone.mountain,
                new Vector3[] {
                    new(-24.1866f, 18.1936f, -95.9086f),
                    new(0f, 250.1091f, 0f)
                }
            },
            {
                GTZone.basement,
                new Vector3[] {
                    new(-36.0703f, 14.4027f, -94.4919f),
                    new(0f, 11.5272f, 0f)
                }
            },
            {
                GTZone.beach,
                new Vector3[] {
                    new(27.21f, 10.2008f, -1.6763f),
                    new(0f, 263.709f, 0f)
                }
            },
            {
                GTZone.skyJungle,
                new Vector3[] {
                    new(-76.7905f, 162.7874f, -100.4427f),
                    new(0f, 342.6743f, 0f)
                }
            },
            {
                GTZone.tutorial,
                new Vector3[] {
                    new(-98.9992f, 37.6046f, -72.6943f),
                    new(0f, 8.7437f, 0f)
                }
            }
        };

        private RigInstance _localRig;

        private Stand _stand;
        private Dictionary<ButtonType, Action> _buttonDict;

        private int _currentPack;
        private List<Pack> _packList = new();

        private Events _events;

        private Shirt CurrentShirt => CurrentPack.PackagedShirts[CurrentPack.CurrentItem];
        private Pack CurrentPack => _packList[_currentPack];

        [Inject]
        public void Construct(AssetLoader assetLoader, Configuration config, Installation shirtInstaller, Networking networking)
        {
            _networking = networking;

            _assetLoader = assetLoader;
            _shirtInstaller = shirtInstaller;
            _config = config;

            _events = new Events();
        }

        public async void Initialize()
        {
            if (_initalized) return;
            _initalized = true;

            // Prepares the dictionary for button actions
            #region Button Initialization

            _buttonDict = new Dictionary<ButtonType, Action>()
            {
                {
                    ButtonType.ShirtEquip,
                    delegate
                    {
                        SetShirt(CurrentShirt);
                        _stand.Display.SetEquipped(CurrentShirt, _localRig.Rig.ActiveShirt);
                    }
                },
                {
                    ButtonType.ShirtLeft,
                    delegate
                    {
                        var currentPack = _packList[_currentPack];
                        currentPack.CurrentItem = currentPack.CurrentItem == 0 ? currentPack.PackagedShirts.Count - 1 : currentPack.CurrentItem - 1;

                        _stand.Rig.Wear(CurrentShirt);
                        _stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);
                    }
                },
                {
                    ButtonType.ShirtRight,
                    delegate
                    {
                        var currentPack = _packList[_currentPack];
                        currentPack.CurrentItem = (currentPack.CurrentItem + 1) % currentPack.PackagedShirts.Count;

                        _stand.Rig.Wear(CurrentShirt);
                        _stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);
                    }
                },
                {
                    ButtonType.PackLeft,
                    delegate
                    {
                        _currentPack = _currentPack == 0 ? _packList.Count - 1 : _currentPack - 1;
                        SetPackInfo(_packList[_currentPack], _packList[_currentPack].PackagedShirts[_packList[_currentPack].CurrentItem]);

                        _stand.Rig.Wear(CurrentShirt);
                        _stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);
                    }
                },
                {
                    ButtonType.PackRight,
                    delegate
                    {
                        _currentPack = (_currentPack + 1) % _packList.Count;
                        SetPackInfo(_packList[_currentPack], _packList[_currentPack].PackagedShirts[_packList[_currentPack].CurrentItem]);

                        _stand.Rig.Wear(CurrentShirt);
                        _stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);
                    }
                },
                {
                    ButtonType.RigToggle,
                    delegate
                    {
                        _config.SetCurrentPreview(_config.CurrentPreview.Value == Configuration.PreviewTypes.Silly, true);
                        _stand.Rig.SetAppearance(_config.CurrentPreview.Value == Configuration.PreviewTypes.Silly);
                    }
                },
                {
                    ButtonType.Randomize,
                    delegate
                    {
                        CurrentPack.Randomize();

                        _stand.Rig.Wear(CurrentShirt);
                        _stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);

                        AudioSource mainSource = _stand.Object.transform.Find("MainSource").GetComponent<AudioSource>();
                        mainSource.clip = _shirtAudioList[5];
                        mainSource.PlayOneShot(mainSource.clip, 1);
                    }
                },
                {
                    ButtonType.TagDecrease,
                    delegate
                    {
                        if (_config.CurrentTagOffset.Value > 0)
                        {
                            _config.CurrentTagOffset.Value--;
                            _networking.UpdateProperties(_networking.GenerateHashtable(_localRig.Rig.ActiveShirt, _config.CurrentTagOffset.Value));
                        }

                        _stand.Rig.SetTagOffset(_config.CurrentTagOffset.Value);
                        _stand.Display.SetTag(_config.CurrentTagOffset.Value);

                        if (_localRig.Rig.ActiveShirt != null) _localRig.Rig.SetTagOffset(_config.CurrentTagOffset.Value);
                    }
                },
                {
                    ButtonType.TagIncrease,
                    delegate
                    {
                        if (_config.CurrentTagOffset.Value < Constants.TagOffsetLimit)
                        {
                            _config.CurrentTagOffset.Value++;
                            _networking.UpdateProperties(_networking.GenerateHashtable(_localRig.Rig.ActiveShirt, _config.CurrentTagOffset.Value));
                        }

                        _stand.Rig.SetTagOffset(_config.CurrentTagOffset.Value);
                        _stand.Display.SetTag(_config.CurrentTagOffset.Value);

                        if (_localRig.Rig.ActiveShirt != null) _localRig.Rig.SetTagOffset(_config.CurrentTagOffset.Value);
                    }
                },
                {
                    ButtonType.AdvancedTab,
                    delegate
                    {
                        _advancedVisible ^= true;
                        OnInfoMenuChanged?.Invoke(_advancedVisible);
                    }
                },
                {
                    ButtonType.Capture,
                    delegate
                    {
                        // Re-enable our camera, just so it has enough time to render
                        _currentCamera.gameObject.SetActive(true);

                        AudioSource mainSource = _stand.Object.transform.Find("MainSource").GetComponent<AudioSource>();
                        mainSource.clip = _shirtAudioList[7];
                        mainSource.PlayOneShot(mainSource.clip, 1);

                        StartCoroutine(Capture());
                        _currentCamera.gameObject.SetActive(false);
                    }
                }
            };

            #endregion

            // Creates the shirt stands used for interaction with the mod
            #region Stand Initialization

            _localRig = GorillaTagger.Instance.offlineVRRig.gameObject.AddComponent<RigInstance>();

            GameObject shirtStand = Instantiate(await _assetLoader.LoadAsset<GameObject>("ShirtStand"));
            shirtStand.name = $"Shirt Stand";
            shirtStand.transform.position = _standLocationData[GTZone.forest][0];
            shirtStand.transform.rotation = Quaternion.Euler(_standLocationData[GTZone.forest][1]);
            AudioSource standAudio = shirtStand.transform.Find("MainSource").GetComponent<AudioSource>();

            ZonePatches.OnMapUpdate += delegate (GTZone zone)
            {
                if (_standLocationData.TryGetValue(zone, out Vector3[] zoneData))
                {
                    shirtStand.transform.position = zoneData[0];
                    shirtStand.transform.rotation = Quaternion.Euler(zoneData[1]);
                }
            };

            StandRig standRig = new()
            {
                Toggle = false,
                RigParent = shirtStand.transform.Find("Preview Gorilla")
            };
            standRig.RigParent.Find("Rig").gameObject.AddComponent<Punch>();

            _currentCamera = shirtStand.transform.Find("Camera").GetComponent<Camera>();
            _currentCamera.gameObject.SetActive(false);

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

                if (_shirtAudioList.Count > 0)
                {
                    standAudio.clip = previewType == Configuration.PreviewTypes.Silly ? _shirtAudioList[3] : _shirtAudioList[4];
                    standAudio.PlayOneShot(standAudio.clip, 1f);
                }
            };

            standRig.OnShirtWorn += delegate
            {
                bool invisibility = standRig.ActiveShirt.Invisibility;

                standRig.RigSkin.forceRenderingOff = invisibility;
                standRig.Head.Find("Face").gameObject.SetActive(!invisibility);
                standRig.Body.Find("Chest").gameObject.SetActive(!invisibility);

                standRig.SillyHat.forceRenderingOff = invisibility || standRig.ActiveShirt.SectorList.Any((a) => a.Type == SectorType.Head);
                standRig.SteadyHat.forceRenderingOff = invisibility || standRig.ActiveShirt.SectorList.Any((a) => a.Type == SectorType.Head);

                standRig.CachedObjects[standRig.ActiveShirt].DoIf(a => a.GetComponentInChildren<AudioSource>(), a =>
                {
                    a.GetComponentsInChildren<AudioSource>().Do(src =>
                    {
                        src.playOnAwake = false;
                        src.Stop();
                    });
                });
            };

            OnInfoMenuChanged += delegate (bool isActive)
            {
                shirtStand.transform.Find("UI/PrimaryDisplay/Text").gameObject.SetActive(!isActive);
                shirtStand.transform.Find("UI/PrimaryDisplay/Info Text").gameObject.SetActive(isActive);

                StringBuilder stringBuilder = new();
                stringBuilder.Append("Shirts: ").Append(_packList.Select((a) => a.PackagedShirts.Count).Sum()).Append(" | Packs: ").Append(_packList.Count);
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
            standRig.SetAppearance(_config.CurrentPreview.Value == Configuration.PreviewTypes.Silly);

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
                    if (_shirtAudioList.Count == 0) return;

                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(Player.Instance.materialData.Count - 1, component.isLeftHand, 0.028f);

                    if (PhotonNetwork.InRoom && GorillaTagger.Instance.myVRRig != null)
                    {
                        GorillaTagger.Instance.myVRRig.RPC("PlayHandTap", (RpcTarget)1, new object[3]
                        {
                            Player.Instance.materialData.Count - 1,
                            component.isLeftHand,
                            0.1f
                        });
                    }

                    if (_packList.Count == 0) return;
                    _buttonDict[UIButton.Type]?.Invoke();
                };

                OnInfoMenuChanged += delegate (bool isActive)
                {
                    ButtonType type = UIButton.Type;
                    UIButton.gameObject.SetActive(!isActive || type == ButtonType.AdvancedTab);
                };
            });
            UIButtonParent.gameObject.SetActive(false);

            _stand = new Stand()
            {
                Display = standDisplay,
                Audio = standAudio,
                Rig = standRig,
                Object = shirtStand
            };

            #endregion

            // Prepares much of the audio-related functions
            #region Audio Initialization

            _shirtAudioList = new List<AudioClip>
            {
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
            };

            Events.PlayShirtAudio += delegate (VRRig p_Rig, int p_AudioIndex, float p_Volume)
            {
                if (p_Rig == null || p_AudioIndex > _shirtAudioList.Count - 1) return;
                p_Rig.tagSound.PlayOneShot(_shirtAudioList[p_AudioIndex], p_Volume);
            };

            Events.PlayCustomAudio += delegate (VRRig p_Rig, AudioClip p_Clip, float p_Volume)
            {
                if (!p_Rig || !p_Clip) return;
                p_Rig.tagSound.PlayOneShot(p_Clip, p_Volume);
            };

            Player.Instance.materialData.Add(new Player.MaterialData()
            {
                overrideAudio = true,
                audio = _shirtAudioList[2],
                matName = "gorillashirtbuttonpress"
            });

            #endregion

            #region Incompatibility Check

            foreach (var pluginInfo in Chainloader.PluginInfos.Values)
            {
                if (pluginInfo.Metadata.GUID == "com.nachoengine.playermodel")
                {
                    shirtStand.transform.Find("UI").GetComponent<Animator>().Play("IncompFrame");
                    return;
                }
            }

            #endregion

            // Loads a set of packs from our plugins directory and attempts to wear our shirt from a previous session
            #region Pack Initialization

            _packList = await _shirtInstaller.FindShirtsFromDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            // Sort the packs based on their name, and prioritize the "Default" pack
            _packList.Sort((x, y) => string.Compare(x.Name, y.Name));
            _packList = _packList.OrderBy(a => a.Name == "Default" ? 0 : 1).ToList();

            foreach (var myPack in _packList)
            {
                if (myPack.DisplayName == "Default") myPack.Randomize();
                else myPack.PackagedShirts.Sort((x, y) => string.Compare(x.Name, y.Name));

                foreach (var myShirt in myPack.PackagedShirts)
                {
                    ShirtUtils.ShirtDict.TryAdd(myShirt.Name, myShirt);
                    if (myShirt.Name == _config.CurrentShirt.Value && _localRig.Rig.ActiveShirt != myShirt)
                    {
                        Logging.Info("Using shirt from previous session '" + myShirt.DisplayName + "' in pack '" + myPack.DisplayName + "'");
                        SetPackInfo(myPack, myShirt);
                        SetShirt(myShirt);
                    }
                }
            }

            WardrobePatches.CosmeticUpdated += delegate (CosmeticsController.CosmeticCategory category)
            {
                bool condition = _localRig.Rig.ActiveShirt != null && _config.RemoveBaseItem.Value && (_localRig.Rig.ActiveShirt.SectorList.Any(a => a.Type == SectorType.Head) && category == CosmeticsController.CosmeticCategory.Hat || category == CosmeticsController.CosmeticCategory.Badge);
                if (condition) SetShirt(_localRig.Rig.ActiveShirt);
            };

            #endregion

            if (_packList != null && _packList.Count > 0)
            {
                standRig.SetTagOffset(_config.CurrentTagOffset.Value);

                // Adjust the stand to display our currently worn shirt
                _stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);
                _stand.Display.SetTag(_config.CurrentTagOffset.Value);
                _stand.Rig.Wear(CurrentShirt);

                UIButtonParent.gameObject.SetActive(true);
                UITextParent.gameObject.SetActive(true);
                shirtStand.transform.Find("UI").GetComponent<Animator>().Play("FadeInFrame");
            }
        }

        public void SetPackInfo(Pack myPack, Shirt myShirt)
        {
            _currentPack = _packList.IndexOf(myPack);
            myPack.CurrentItem = myPack.PackagedShirts.IndexOf(myShirt);
        }

        public void SetShirt(Shirt myShirt)
        {
            if (myShirt != null)
            {
                _localRig.Rig.Wear(myShirt, out Shirt preShirt, out Shirt postShirt);
                _localRig.Rig.SetTagOffset(postShirt != null ? _config.CurrentTagOffset.Value : 0);

                // Cosmetics
                if (postShirt != null && _config.RemoveBaseItem.Value)
                {
                    ShirtUtils.RemoveItem(CosmeticsController.CosmeticCategory.Badge, CosmeticsController.CosmeticSlots.Badge);
                    if (postShirt.SectorList.Any(a => a.Type == SectorType.Head)) ShirtUtils.RemoveItem(CosmeticsController.CosmeticCategory.Hat, CosmeticsController.CosmeticSlots.Hat);
                }

                // Audio
                if (postShirt != null)
                {
                    if (postShirt.Wear) _events.TriggerPlayCustomAudio(_localRig.GetComponent<VRRig>(), postShirt.Wear, 0.3f);
                    else _events.TriggerPlayShirtAudio(_localRig.GetComponent<VRRig>(), 0, 0.4f);
                }
                else if (preShirt != null)
                {
                    if (preShirt.Remove) _events.TriggerPlayCustomAudio(_localRig.GetComponent<VRRig>(), preShirt.Remove, 0.3f);
                    else _events.TriggerPlayShirtAudio(_localRig.GetComponent<VRRig>(), 1, 0.4f);
                }

                // Networking
                _networking.UpdateProperties(_networking.GenerateHashtable(_localRig.Rig.ActiveShirt, _config.CurrentTagOffset.Value));

                // Configuration
                _config.SetCurrentShirt(postShirt != null ? myShirt.Name : "None");
            }
        }

        public IEnumerator Capture()
        {
            string directory = Path.Combine(Paths.BepInExRootPath, "GorillaShirts Captures");
            string file = Path.Combine(directory, string.Format("{0:yy-MM-dd-HH-mm-ss-ff}.png", DateTime.Now));
            _shirtInstaller.TryCreateDirectory(directory);

            yield return new WaitForEndOfFrame();

            RenderTexture renderTexture = _currentCamera.targetTexture;
            RenderTexture.active = renderTexture;
            int width = renderTexture.width;
            int height = renderTexture.height;

            RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
            Texture2D tex = new(width, height, TextureFormat.RGB24, false);

            RenderTexture.active = renderTex;
            _currentCamera.targetTexture = renderTex;

            _currentCamera.Render();
            _currentCamera.targetTexture = renderTexture;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTex);

            File.WriteAllBytes(file, tex.EncodeToPNG());
            yield break;
        }

        public override async void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            await Task.Delay(Mathf.Max(PhotonNetwork.GetPing(), Constants.NetworkOffset));

            _networking.UpdateProperties(_networking.GenerateHashtable(_localRig.Rig.ActiveShirt, _config.CurrentTagOffset.Value));
        }
    }
}