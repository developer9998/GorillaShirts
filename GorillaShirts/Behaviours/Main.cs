using BepInEx;
using BepInEx.Bootstrap;
using GorillaExtensions;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Buttons;
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

        public readonly List<IStandButton> StandButtons =
        [
            new ShirtEquip(),
            new ShirtIncrease(),
            new ShirtDecrease(),
            new PackIncrease(),
            new PackDecrease(),
            new RigToggle(),
            new Randomize(),
            new TagIncrease(),
            new TagDecrease(),
            new Information(),
            new Capture()
        ];

        public readonly List<IStandLocation> StandLocations =
        [
            new Forest(),
            new Cave(),
            new Canyon(),
            new City(),
            new Mountain(),
            new Clouds(),
            new Basement(),
            new Beach(),
            new Tutorial(),
            new Rotating(),
            new Metropolis(),
            new Arcade(),
            new Bayou(),
            new VirtualStump(),
            new Mall(),
            new MonkeBlocks(),
            new Mines()
        ];

        public int SelectedPackIndex;

        public List<Pack> Packs = [];

        public Shirt SelectedShirt
        {
            get => SelectedPack.PackagedShirts[SelectedPack.CurrentItem];
            set
            {
                foreach (var pack in Packs)
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
            get => Packs[SelectedPackIndex];
            set => SelectedPackIndex = Packs.IndexOf(value);
        }

        private AssetLoader asset_loader;
        private ShirtReader shirt_reader;
        private List<AudioClip> audio_clips = [];

        public Hashtable CustomProperties;

        private bool to_update_properties;
        private float last_property_update;

        // april fools
        public DateTime MyTime => TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time"));
        public bool April1 => MyTime.Month == 4 && (MyTime.Day == 1 || MyTime.Day <= 7);

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
            else if (requestVersion.downloadHandler.text.TrimEnd() != Constants.Version)
            {
                Logging.Warning($"GitHub version string mismatch, came back with {requestVersion.downloadHandler.text} expecting {Constants.Version}");
                return;
            }

            asset_loader = new AssetLoader();
            shirt_reader = new ShirtReader();

            await InitAll();

            if (Packs != null && Packs.Count > 0)
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

            GameObject shirtStand = Instantiate(await asset_loader.LoadAsset<GameObject>("ShirtStand"));
            shirtStand.name = "Shirt Stand";
            shirtStand.transform.SetParent(transform);
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

                if (audio_clips.Count > 0)
                {
                    standAudio.clip = previewType == Configuration.PreviewGorilla.Silly ? audio_clips[3] : audio_clips[4];
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
                stringBuilder.Append("Shirts: ").Append(Packs.Select((a) => a.PackagedShirts.Count).Sum()).Append(" | Packs: ").Append(Packs.Count);
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
                    if (!audio_clips.Any()) return;

                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(GorillaLocomotion.GTPlayer.Instance.materialData.Count - 1, component.isLeftHand, 0.028f);

                    if (!Packs.Any()) return;
                    StandButtons.Find(button => button.Type == UIButton.Type)?.Function?.Invoke(this);
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
            MoveStand(StandLocations.First().Location.Item1, StandLocations.First().Location.Item2);
        }

        private void OnZoneChange(ZoneData[] zones)
        {
            IEnumerable<GTZone> activeZones = zones.Where(zone => zone.active).Select(zone => zone.zone);
            OnZoneChange(activeZones.ToArray());
        }

        public void OnZoneChange(GTZone[] zones)
        {
            //Logging.Info($"Zone changed: {string.Join(", ", zones)}");

            foreach (GTZone currentZone in zones)
            {
                IStandLocation currentLocation = StandLocations.FirstOrDefault(zone => zone.IsInZone(currentZone));

                if (currentLocation != null)
                {
                    //Logging.Info($"We are in {currentLocation.GetType().Name} ({currentZone} is active)");

                    Tuple<Vector3, Vector3> locationData = currentLocation.Location;
                    MoveStand(locationData.Item1, locationData.Item2, currentLocation.Roof);
                    return;
                }
            }

            Logging.Warning($"No stand location exists for zones {string.Join(", ", zones)}, hiding shirt stand");
            Stand.Object.SetActive(false);
        }

        public void MoveStand(Transform transform) => MoveStand(transform.position, transform.eulerAngles);

        public void MoveStand(Vector3 position, Vector3 direction, float height = -1)
        {
            Stand.Object.transform.position = position;
            Stand.Object.transform.rotation = Quaternion.Euler(direction);
            if (April1)
            {
                Stand.Object.transform.Rotate(0, 0, 180, Space.Self);
                Stand.Object.transform.position = position.WithY(height == -1 ? position.y : height);
            }
            Stand.Object.SetActive(true);
        }

        public async Task InitAudio()
        {
            audio_clips =
            [
                await asset_loader.LoadAsset<AudioClip>("Wear"),
                await asset_loader.LoadAsset<AudioClip>("Remove"),
                await asset_loader.LoadAsset<AudioClip>("Button"),
                await asset_loader.LoadAsset<AudioClip>("SillyTXT"),
                await asset_loader.LoadAsset<AudioClip>("SteadyTXT"),
                await asset_loader.LoadAsset<AudioClip>("Randomize"),
                await asset_loader.LoadAsset<AudioClip>("Error"),
                await asset_loader.LoadAsset<AudioClip>("Shutter"),
                await asset_loader.LoadAsset<AudioClip>("PackOpen"),
                await asset_loader.LoadAsset<AudioClip>("PackClose"),
            ];

            // i can't remember if i still need this or not
            GorillaLocomotion.GTPlayer.Instance.materialData.Add(new GorillaLocomotion.GTPlayer.MaterialData()
            {
                overrideAudio = true,
                audio = audio_clips[2],
                matName = "gorillashirtbuttonpress"
            });
        }

        public void PlayShirtAudio(VRRig playerRig, int index, float volume)
        {
            if (playerRig == null || index >= audio_clips.Count) return;
            if (April1) index = UnityEngine.Random.Range(0, audio_clips.Count);
            playerRig.tagSound.PlayOneShot(audio_clips[index], volume);
        }

        public void PlayCustomAudio(VRRig playerRig, AudioClip clip, float volume)
        {
            if (!playerRig || !clip) return;
            playerRig.tagSound.PlayOneShot(clip, volume);
        }

        public bool IsIncompatibile()
        {
            try
            {
                foreach (var pluginInfo in Chainloader.PluginInfos.Values)
                {
                    Assembly pluginAssembly = pluginInfo.Instance.GetType().Assembly;
                    var pluginTypes = pluginAssembly.GetTypes();
                    if (pluginInfo.Metadata.GUID == "com.wryser.gorillatag.customcosmetics" || pluginInfo.Metadata.GUID == "com.goldentrophy.gorillatag.fortniteemotewheel" || pluginTypes.Any(type => type.Name.Contains("WristMenu") || type.Name.Contains("MenuPatch") || type.Name.Contains("Cosmetx") || type.Name.Contains("RigPatch2")))
                    {
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                Logging.Error($"Incomp check threw exception: {ex}");
                Logging.Warning("returning w true despite checks");
            }
            return false;
        }

        public async Task InitCatalog()
        {
            Packs = await shirt_reader.FindShirtsFromDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            if (Packs == null || Packs.Count == 0)
            {
                Logging.Warning("No packs found");
                Destroy(Stand.Object);
                return;
            }

            // Sort the packs based on their name, and prioritize the "Default" pack
            Packs.Sort((x, y) => string.Compare(x.Name, y.Name));
            Packs = [.. Packs.OrderBy(a => a.Name == "Default" ? 0 : 1)];

            foreach (var myPack in Packs)
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
            int audio_index = (int)audio;
            if (April1) audio_index = UnityEngine.Random.Range(0, audio_clips.Count);
            mainSource.clip = audio_clips[audio_index];
            mainSource.PlayOneShot(mainSource.clip, volume);
        }

        public void SetPackInfo(Pack myPack, Shirt myShirt)
        {
            SelectedPackIndex = Packs.IndexOf(myPack);
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
            if (to_update_properties && Time.unscaledTime > last_property_update
)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(CustomProperties);

                to_update_properties = false;
                last_property_update
     = Time.unscaledTime + Constants.NetworkCooldown;
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

            to_update_properties = true;

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

            GetShirtRig(player, playerRig);
            Logging.Info($"Added ShirtRig to player {player.NickName}");
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

            Destroy(shirtRig);
            Logging.Info($"Removed ShirtRig from player {player.NickName}");
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
                    bool doMissingCheck = false;

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

                        doMissingCheck = true;
                    }

                    if (!doMissingCheck) return;

                    if (wornGorillaShirt != null && !string.IsNullOrEmpty(wornGorillaShirt))
                    {
                        Logging.Info($"{targetPlayer.NickName} is wearing shirt {wornGorillaShirt} (missing)");

                        // play the shirt missing audio
                        PlayShirtAudio(playerRig, 6, 1f);

                        return;
                    }

                    //Logging.Warning($"This should not happen ({targetPlayer.NickName}, {wornGorillaShirt})");
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Failed to handle custom props from player {targetPlayer.NickName}: {ex}");
            }
        }
    }
}