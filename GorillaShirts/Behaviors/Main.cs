using GorillaNetworking;
using GorillaShirts.Behaviors.Data;
using GorillaShirts.Behaviors.Interaction;
using GorillaShirts.Behaviors.Models;
using GorillaShirts.Behaviors.Tools;
using GorillaShirts.Behaviors.UI;
using GorillaShirts.Extensions;
using GorillaShirts.Utilities;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Button = GorillaShirts.Behaviors.Interaction.Button;

namespace GorillaShirts.Behaviors
{
    public class Main : MonoBehaviourPunCallbacks, IInitializable
    {
        private bool _initalized;

        private Networking _networking;

        private AssetLoader _assetLoader;

        private Installation _shirtInstaller;
        private Configuration _config;

        private event Action<bool> _advancedState;
        private bool _advancedVisible;

        private List<AudioClip> _shirtAudioList;
        private Dictionary<GTZone, Vector3[]> _standLocationData = new()
        {
            {
                GTZone.forest,
                new Vector3[] {
                    new Vector3(-67.6651f, 12.07f, -80.438f),
                    new Vector3(0f, 171.1801f, 0f)
                }
            },
            {
                GTZone.cave,
                new Vector3[] {
                    new Vector3(-60.525f, -11.7064f, -41.7656f),
                    new Vector3(0f, 267.9607f, 0f)
                }
            },
            {
                GTZone.canyon,
                new Vector3[] {
                    new Vector3(-100.246f, 17.9809f, -169.374f),
                    new Vector3(0f, 297.5797f, 0f)
                }
            },
            {
                GTZone.city,
                new Vector3[] {
                    new Vector3(-56.8213f, 17.0026f, -120.0597f),
                    new Vector3(0f, 316.1757f, 0f)
                }
            },
            {
                GTZone.mountain,
                new Vector3[] {
                    new Vector3(-24.1866f, 18.1936f, -95.9086f),
                    new Vector3(0f, 250.1091f, 0f)
                }
            },
            {
                GTZone.basement,
                new Vector3[] {
                    new Vector3(-34.8884f, 14.4027f, -95.1101f),
                    new Vector3(0f, 11.5272f, 0f)
                }
            },
            {
                GTZone.beach,
                new Vector3[] {
                    new Vector3(27.21f, 10.2008f, -1.6763f),
                    new Vector3(0f, 263.709f, 0f)
                }
            },
            {
                GTZone.skyJungle,
                new Vector3[] {
                    new Vector3(-76.7905f, 162.7874f, -100.4427f),
                    new Vector3(0f, 342.6743f, 0f)
                }
            },
            {
                GTZone.tutorial,
                new Vector3[] {
                    new Vector3(-98.9992f, 37.6046f, -72.6943f),
                    new Vector3(0f, 8.7437f, 0f)
                }
            }
        };

        private RigInstance _localRig;

        private List<Stand> _standList = new();
        private Dictionary<ButtonType, Action> _buttonDict;

        private int _currentPack;
        private List<Pack> _packList = new();

        private Events _Events;

        private Shirt CurrentShirt => CurrentPack.PackagedShirts[CurrentPack.CurrentItem];
        private Pack CurrentPack => _packList[_currentPack];

        [Inject]
        public void Construct(AssetLoader assetLoader, Configuration config, Installation shirtInstaller, Networking networking)
        {
            _networking = networking;

            _assetLoader = assetLoader;
            _shirtInstaller = shirtInstaller;
            _config = config;
        }

        public async void Initialize()
        {
            if (_initalized) return;
            _initalized = true;

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
                await _assetLoader.LoadAsset<AudioClip>("Error")
            };

            Events.PlayShirtAudio += delegate (VRRig vrRig, int index, float volume)
            {
                if (vrRig == null || index > (_shirtAudioList.Count - 1)) return;
                vrRig.tagSound.PlayOneShot(_shirtAudioList[index], volume);
            };
            #endregion

            // Prepares the dictionary for button actions
            #region Button Initialization
            _buttonDict = new Dictionary<ButtonType, Action>()
            {
                {
                    ButtonType.ShirtEquip,
                    delegate
                    {
                        SetShirt(CurrentShirt);
                        _standList.ForEach(stand => stand.Display.SetEquipped(CurrentShirt, _localRig.Rig.ActiveShirt));
                    }
                },
                {
                    ButtonType.ShirtLeft,
                    delegate
                    {
                        var currentPack = _packList[_currentPack];
                        currentPack.CurrentItem = currentPack.CurrentItem == 0 ? currentPack.PackagedShirts.Count - 1 : currentPack.CurrentItem - 1;

                        _standList.ForEach(stand =>
                        {
                            stand.Rig.Wear(CurrentShirt);
                            stand.Display.SetDisplay(CurrentShirt);
                            stand.Display.SetSlots(CurrentShirt.GetSlotData());
                            stand.Display.SetEquipped(CurrentShirt, _localRig.Rig.ActiveShirt);
                        });
                    }
                },
                {
                    ButtonType.ShirtRight,
                    delegate
                    {
                        var currentPack = _packList[_currentPack];
                        currentPack.CurrentItem = (currentPack.CurrentItem + 1) % currentPack.PackagedShirts.Count;

                        _standList.ForEach(stand =>
                        {
                            stand.Rig.Wear(CurrentShirt);
                            stand.Display.SetDisplay(CurrentShirt);
                            stand.Display.SetSlots(CurrentShirt.GetSlotData());
                            stand.Display.SetEquipped(CurrentShirt, _localRig.Rig.ActiveShirt);
                        });
                    }
                },
                {
                    ButtonType.PackLeft,
                    delegate
                    {
                        _currentPack = _currentPack == 0 ? _packList.Count - 1 : _currentPack - 1;
                        SetPackInfo(_packList[_currentPack], _packList[_currentPack].PackagedShirts[_packList[_currentPack].CurrentItem]);

                        _standList.ForEach(stand =>
                        {
                            stand.Rig.Wear(CurrentShirt);
                            stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);
                        });
                    }
                },
                {
                    ButtonType.PackRight,
                    delegate
                    {
                        _currentPack = (_currentPack + 1) % _packList.Count;
                        SetPackInfo(_packList[_currentPack], _packList[_currentPack].PackagedShirts[_packList[_currentPack].CurrentItem]);

                        _standList.ForEach(stand =>
                        {
                            stand.Rig.Wear(CurrentShirt);
                            stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);
                        });
                    }
                },
                {
                    ButtonType.RigToggle,
                    delegate
                    {
                        _config.SetCurrentPreview(_config.CurrentPreview.Value == Configuration.PreviewTypes.Silly, true);

                        _standList.ForEach(stand =>
                        {
                            stand.Rig.SetAppearance(_config.CurrentPreview.Value == Configuration.PreviewTypes.Silly);
                            stand.Rig.Wear(CurrentShirt);
                            stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);
                        });
                    }
                },
                {
                    ButtonType.Randomize,
                    delegate
                    {
                        CurrentPack.Randomize();

                        _standList.ForEach(stand =>
                        {
                            stand.Rig.Wear(CurrentShirt);
                            stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);

                            AudioSource mainSource = stand.Object.transform.Find("MainSource").GetComponent<AudioSource>();
                            mainSource.clip = _shirtAudioList[5];
                            mainSource.PlayOneShot(mainSource.clip, 1);
                        });
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

                        _standList.ForEach(stand =>
                        {
                            stand.Rig.SetTagOffset(_config.CurrentTagOffset.Value);
                            stand.Display.SetTag(_config.CurrentTagOffset.Value);
                        });
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

                        _standList.ForEach(stand =>
                        {
                            stand.Rig.SetTagOffset(_config.CurrentTagOffset.Value);
                            stand.Display.SetTag(_config.CurrentTagOffset.Value);
                        });
                        if (_localRig.Rig.ActiveShirt != null) _localRig.Rig.SetTagOffset(_config.CurrentTagOffset.Value);
                    }
                },
                {
                    ButtonType.AdvancedTab,
                    delegate
                    {
                        _advancedVisible ^= true;
                        _advancedState?.Invoke(_advancedVisible);
                    }
                }
            };
            #endregion

            // Creates the shirt stands used for interaction with the mod
            #region Stand Initialization
            // Prepare the stands placed around the game
            var _locations = _standLocationData.Keys.ToArray();
            for (int i = 0; i < _locations.Length; i++)
            {
                try
                {
                    float _currentTime = Time.unscaledTime;
                    Stand _stand = await CreateStand(_locations[i], _standLocationData[_locations[i]]);
                    _standList.Add(_stand);

                    Logging.Log($"Created stand for {_locations[i]} in {Math.Round(Mathf.Abs(Time.unscaledTime - _currentTime), 2)} seconds");
                }
                catch (Exception e)
                {
                    Logging.Error($"Unable to create stand for {_locations[i]}\n{e.GetType().Name} ({e.Message})");
                }
            }
            #endregion

            _localRig = GorillaTagger.Instance.offlineVRRig.gameObject.AddComponent<RigInstance>();

            // Loads a set of packs from our plugins directory and attempts to wear our shirt from a previous session
            #region Pack Initialization
            _packList = await _shirtInstaller.FindShirtsFromDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            foreach (var myPack in _packList)
            {
                if (myPack.DisplayName == "Shirts") myPack.Randomize();
                if (_config.CurrentShirt.Value == "None" || myPack.PackagedShirts.Count == 0) continue;

                foreach (var myShirt in myPack.PackagedShirts)
                {
                    ShirtUtils.ShirtDict.Add(myShirt.Name, myShirt);
                    if (myShirt.Name == _config.CurrentShirt.Value && _localRig.Rig.ActiveShirt != myShirt)
                    {
                        Logging.Log("Found Shirt from previous session (" + myShirt.DisplayName + ") In Pack " + myShirt.DisplayName);
                        SetPackInfo(myPack, myShirt);
                        SetShirt(myShirt);
                    }
                }
            }
            #endregion

            if (_packList == null || _packList.Count == 0)
            {
                _standList.ForEach(stand =>
                {
                    stand.Object.transform.Find("Activation Source").GetComponent<AudioSource>().PlayOneShot(_shirtAudioList[6]);

                    StringBuilder str = new();
                    str.Append("<size=10>- No shirts found! -</size>").AppendLines(3);
                    str.AppendLine("The mod could not find any shirts.").AppendLines(2);
                    str.AppendLine("If you believe the mod ran into").AppendLine();
                    str.AppendLine("an error, please let us know:").AppendLines(2);
                    str.Append("Discord: <size=5>discord.gg/dev9998</size>");

                    stand.Display.Main.text = str.ToString();
                });
            }
            else
            {
                _standList.ForEach(stand =>
                {
                    stand.Object.transform.Find("Activation Source").GetComponent<AudioSource>().Play();

                    // Adjust the stand to display our currently worn shirt
                    stand.Display.UpdateDisplay(CurrentShirt, _localRig.Rig.ActiveShirt, CurrentPack);
                    stand.Display.SetTag(_config.CurrentTagOffset.Value);
                    stand.Rig.Wear(CurrentShirt);
                });
            }
        }

        public void SetPackInfo(Pack myPack, Shirt myShirt)
        {
            _currentPack = _packList.IndexOf(myPack);
            myPack.CurrentItem = myPack.PackagedShirts.IndexOf(myShirt);
        }

        public void SetShirt(Shirt myShirt)
        {
            if (myShirt == null) return;

            // Equip this shirt, and set our nametag offset accordingly
            _localRig.Rig.Wear(myShirt);
            _localRig.Rig.SetTagOffset(_localRig.Rig.ActiveShirt != null ? _config.CurrentTagOffset.Value : 0);

            // Update our configuration to take note of our current shirt
            _config.SetCurrentShirt(_localRig.Rig.ActiveShirt != null ? myShirt.Name : "None");

            // Remove our current badge item if we're both wearing a shirt and our confiruration has that option enabled
            if (_config.RemoveBaseItem.Value && _localRig.Rig.ActiveShirt != null)
                ShirtUtils.RemoveItem(GorillaNetworking.CosmeticsController.CosmeticCategory.Badge, GorillaNetworking.CosmeticsController.CosmeticSlots.Badge);

            // Notify our events to play a sound based on if we're wearing a shirt
            _Events ??= new Events();
            _Events.TriggerPlayShirtAudio(_localRig.GetComponent<VRRig>(), _localRig.Rig.ActiveShirt != null ? 0 : 1, 0.33f);

            // Update our CustomProperties which will network our shirt for other players
            _networking.UpdateProperties(_networking.GenerateHashtable(_localRig.Rig.ActiveShirt, _config.CurrentTagOffset.Value));
        }

        public async Task<Stand> CreateStand(GTZone _zone, Vector3[] _locationData)
        {
            var shirtStand = Instantiate(await _assetLoader.LoadAsset<GameObject>(Constants.StandName));
            shirtStand.name = $"Shirt Stand ({_zone})";
            shirtStand.transform.position = _locationData[0];
            shirtStand.transform.rotation = Quaternion.Euler(_locationData[1]);

            var standRig = new StandRig
            {
                Toggle = false,
                RigParent = shirtStand.transform.Find("Preview Gorilla")
            };
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

            // Add additional components
            standRig.RigParent.Find("Rig").gameObject.AddComponent<Punch>();
            #region Prepare IK (Inverse Kinematics)

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

            // Manually do this part - Awake doesn't work out
            gorillaIk.dU = (gorillaIk.leftUpperArm.position - gorillaIk.leftLowerArm.position).magnitude;
            gorillaIk.dL = (gorillaIk.leftLowerArm.position - gorillaIk.leftHand.position).magnitude;
            gorillaIk.dMax = gorillaIk.dU + gorillaIk.dL - gorillaIk.eps;
            gorillaIk.initialUpperLeft = gorillaIk.leftUpperArm.localRotation;
            gorillaIk.initialLowerLeft = gorillaIk.leftLowerArm.localRotation;
            gorillaIk.initialUpperRight = gorillaIk.rightUpperArm.localRotation;
            gorillaIk.initialLowerRight = gorillaIk.rightLowerArm.localRotation;
            gorillaIk.enabled = false;
            gorillaIk.enabled = true;

            #endregion

            standRig.OnAppearanceChange += delegate (Configuration.PreviewTypes previewType)
            {
                // Update the stand based on our current preview type
                shirtStand.transform.Find("UI/PrimaryDisplay/Text/Interaction/Silly Icon").gameObject.SetActive(previewType == Configuration.PreviewTypes.Silly);
                shirtStand.transform.Find("UI/PrimaryDisplay/Text/Interaction/Steady Icon").gameObject.SetActive(previewType == Configuration.PreviewTypes.Steady);

                AudioSource mainSource = shirtStand.transform.Find("MainSource").GetComponent<AudioSource>();
                mainSource.clip = previewType == Configuration.PreviewTypes.Silly ? _shirtAudioList[3] : _shirtAudioList[4];
                mainSource.PlayOneShot(mainSource.clip, 1);
            };

            standRig.ShirtWorn += delegate ()
            {
                bool isActivated = standRig.ActiveShirt.Invisibility;
                standRig.RigSkin.forceRenderingOff = isActivated;
                standRig.Head.Find("Face").gameObject.SetActive(!isActivated);
                standRig.Body.Find("Chest").gameObject.SetActive(!isActivated);
                standRig.SillyHat.forceRenderingOff = isActivated;
                standRig.SteadyHat.forceRenderingOff = isActivated;
            };

            _advancedState += delegate (bool isActive)
            {
                // Update the stand based on our current advanced state
                shirtStand.transform.Find("UI/PrimaryDisplay/Text").gameObject.SetActive(!isActive);
                shirtStand.transform.Find("UI/PrimaryDisplay/AdvancedText").gameObject.SetActive(isActive);

                StringBuilder str = new();
                str.Append("Shirts: ").Append(_packList.Select(a => a.PackagedShirts.Count).Sum()).AppendLine();
                str.Append("Packs: ").Append(_packList.Count);
                shirtStand.transform.Find("UI/PrimaryDisplay/AdvancedText/Left Body").GetComponent<Text>().text = str.ToString();

                str = new();
                str.Append("Build Type: ");
#if DEBUG
                str.AppendLine("Debug");
#else
                str.AppendLine("Release");
#endif
                str.Append("Build Version: ").Append(Constants.Version);
                shirtStand.transform.Find("UI/PrimaryDisplay/AdvancedText/Right Body").GetComponent<Text>().text = str.ToString();
            };

            standRig.SillyHat = standRig.Head.Find("SillyFlowerCrown").GetComponent<MeshRenderer>();
            standRig.SteadyHat = standRig.Head.Find("SteadyHeadphones").GetComponent<MeshRenderer>();
            standRig.SetAppearance(_config.CurrentPreview.Value == Configuration.PreviewTypes.Silly);
            standRig.SetTagOffset(_config.CurrentTagOffset.Value);

            var uiDisplayParent = shirtStand.transform.Find("UI/PrimaryDisplay/Text");
            var standDisplay = new ShirtDisplay
            {
                Main = uiDisplayParent.Find("Main").GetComponent<Text>(),
                Body = uiDisplayParent.Find("Body").GetComponent<Text>(),
                Version = uiDisplayParent.Find("Version").GetComponent<Text>(),
                Equip = uiDisplayParent.Find("Interaction/Equip").GetComponent<Text>(),
                Pack = uiDisplayParent.Find("Interaction/Pack").GetComponent<Text>(),
                Tag = uiDisplayParent.Find("Interaction/Nametag").GetComponent<Text>(),
                SlotParent = uiDisplayParent.Find("Interaction/Slots").gameObject
            };

            for (int x = 0; x < standDisplay.SlotParent.transform.childCount; x++)
            {
                var slotItem = standDisplay.SlotParent.transform.GetChild(x);
                standDisplay.SlotItems.Add(slotItem.gameObject);
            }
            standDisplay.SetSlots(null);

            // Primary button selection
            var uiButtonsParent = shirtStand.transform.Find("UI/PrimaryDisplay/Buttons");
            var buttonList = uiButtonsParent.GetComponentsInChildren<BoxCollider>().ToList();
            buttonList.ForEach(a =>
            {
                var newButton = a.gameObject.AddComponent<Button>();
                newButton.btnType = Button.GetButtonType(a.name);
                newButton.btnAction += delegate (GorillaTriggerColliderHandIndicator component)
                {
                    var audioSource = component.isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
                    audioSource.clip = _shirtAudioList[2];
                    audioSource.PlayOneShot(audioSource.clip, 0.44f);

                    ButtonType currentType = newButton.btnType;
                    if (_buttonDict.TryGetValue(currentType, out Action value))
                    {
                        if (_packList.Count == 0) return;
                        value.Invoke();
                    }
                };
                _advancedState += delegate (bool isActive)
                {
                    ButtonType currentType = newButton.btnType;
                    newButton.gameObject.SetActive(!isActive || currentType == ButtonType.AdvancedTab);
                };
            });

            // Top button selection
            uiButtonsParent = shirtStand.transform.Find("UI/TopSelector/Buttons");
            buttonList = uiButtonsParent.GetComponentsInChildren<BoxCollider>().ToList();
            buttonList.ForEach(a =>
            {
                var newButton = a.gameObject.AddComponent<Button>();
                newButton.btnType = Button.GetButtonType(a.name);
                newButton.btnAction += delegate (GorillaTriggerColliderHandIndicator component)
                {
                    var audioSource = component.isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
                    audioSource.clip = _shirtAudioList[2];
                    audioSource.PlayOneShot(audioSource.clip, 0.44f);

                    ButtonType currentType = newButton.btnType;
                    if (_buttonDict.TryGetValue(currentType, out Action value))
                    {
                        if (_packList.Count == 0) return;
                        value.Invoke();
                    }
                };
                _advancedState += delegate (bool isActive)
                {
                    ButtonType currentType = newButton.btnType;
                    newButton.gameObject.SetActive(!isActive || currentType == ButtonType.AdvancedTab);
                };
            });

            StringBuilder str = new StringBuilder().Append("<size=10>- Loading Shirts -</size>").AppendLines(4);
            str.Append("The mod is currently loading in").AppendLines(2).Append("Shirts found within your files.").AppendLines(3);
            str.Append("Please wait for these files to").AppendLines(2).Append("be loaded into the mod.");
            standDisplay.SetDisplay(str.ToString(), string.Empty);
            standDisplay.SetVersion("v" + "1.0.0");

            return new Stand()
            {
                Object = shirtStand,
                Display = standDisplay,
                Rig = standRig
            };
        }

        public override async void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            await Task.Delay(Mathf.Max(PhotonNetwork.GetPing(), Constants.NetworkOffset));

            _networking.UpdateProperties(_networking.GenerateHashtable(_localRig.Rig.ActiveShirt, _config.CurrentTagOffset.Value));
        }
    }
}