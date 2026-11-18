using GorillaShirts.Behaviours.Appearance;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if PLUGIN
using Newtonsoft.Json;
using System.Threading.Tasks;
using GorillaShirts.Extensions;
using System;
using GorillaShirts.Models.Locations;
using System.Collections.Generic;
using System.Linq;
using GorillaShirts.Models.UI;
#if DEBUG
using System.IO;
using System.Reflection;
#else
using UnityEngine.Networking;
#endif
#endif

namespace GorillaShirts.Behaviours.UI
{
    internal class Stand : MonoBehaviour
    {
        public GameObject Root;

        public AudioSource AudioDevice;

        public Camera Camera;

        public StandCharacterHumanoid Character;

        public GameObject interfaceRoot;

        [Header("Welcome Menu")]

        public GameObject welcomeMenuRoot;

        [Header("Load Menu")]

        public GameObject loadMenuRoot;

        public UnityEngine.UI.Slider loadSlider;

        //public Image loadRadial;

        public TMP_Text loadPercent;

        public TMP_Text didYouKnowText;

        public TMP_Text flagText;

        public GameObject greenFlag;

        public GameObject redFlag;

        [Header("Version Notice Screen")]

        public GameObject versionMenuRoot;

        public TMP_Text versionDiffText;

        [TextArea(2, 4)]
        public string versionDiffFormat;

        public GameObject softVersionContainer, hardVersionContainer;

        [Header("Main Menu")]

        public GameObject mainMenuRoot;

        public GameObject navigationRoot;

        public GameObject mainContentRoot;

        public TMP_Text navigationText;

        public GameObject packBrowserNewSymbol;

        public TMP_Text headerText;

        [TextArea(2, 8)]
        public string headerFormat;

        public TMP_Text descriptionText;

        public Image previewImage;

        public GameObject[] featureObjects;

        public TMP_Text shirtStatusText;

        public Sidebar mainSideBar;

        public GameObject infoButtonObject;

        [Header("Main Menu (info)")]

        public GameObject infoContentRoot;

        public TMP_Text playerInfoText;

        [TextArea(4, 6), FormerlySerializedAs("personalDataFormat")]
        public string playerInfoFormat;

        [Header("Main Menu - Colour Manager")]

        public GameObject mainMenu_colourSubMenu;

        public TMP_Text colourPicker_NavText;

        public ColourPicker colourPicker;

        public GameObject colourPicker_SyncButton, colourPicker_ApplyButton;

        [Header("Pack Browser Load Menu")]

        public GameObject packBrowserMenuRoot;

        public Image packBrowserRadial;

        public TMP_Text packBrowserPercent;

        public TMP_Text packBrowserStatus;

        public TMP_Text packBrowserLabel;

        [Header("UNUSED FOR NOW")] // [Header("Main Menu (under Info tab)")]

        public GameObject tutorialRoot;

        public GameObject[] tutorialTabs;

        public GameObject statisticsRoot;

        public GameObject creditsRoot;

#if PLUGIN

        private readonly Dictionary<GTZone, StandLocation> _locationDictionary = [];

        private bool _useUberMaterials = false;

        private Renderer[] _standRenderers;

        private Dictionary<Renderer, Material[]> _baseMaterials, _uberMaterials;

        public void Start()
        {
            _standRenderers = Root.GetComponentsInChildren<Renderer>(true);
            _baseMaterials = _standRenderers.ToDictionary(renderer => renderer, renderer => renderer.materials);
            _uberMaterials = _standRenderers.ToDictionary(renderer => renderer, renderer => renderer.materials.Select(material => material.CreateUberMaterial(true, true)).ToArray());
            SetMaterialState(Shader.IsKeywordEnabled("_ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX"));

            SetVisibility(Plugin.StandCulling.Value);
            Plugin.StandCulling.SettingChanged += (_, _) => SetVisibility(Plugin.StandCulling.Value);

            InitializeZoneData();
        }

        private async void InitializeZoneData()
        {
            JsonSerializerSettings settings = new()
            {
                CheckAdditionalContent = true,
                Formatting = Formatting.None
            };

            string mapsText = await GetContent("GorillaShirts/Content/Maps.json");
            Dictionary<string, StandLocation> mapsDict = JsonConvert.DeserializeObject<Dictionary<string, StandLocation>>(mapsText, settings);
            foreach (var kvp in mapsDict)
            {
                if (!Enum.TryParse(kvp.Key, out GTZone zone))
                {
                    Plugin.Logger.LogWarning($"Text '{kvp.Key}' does not fit into GTZone enum");
                    continue;
                }

                _locationDictionary.TryAdd(zone, kvp.Value);
            }

            string mapOverridesText = await GetContent("GorillaShirts/Content/MapOverrides.json");
            Dictionary<string, string[]> mapOverrideDict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(mapOverridesText, settings);
            foreach (var kvp in mapOverrideDict)
            {
                if (!Enum.TryParse(kvp.Key, out GTZone baseZone))
                {
                    Plugin.Logger.LogWarning($"Text '{kvp.Key}' does not fit into GTZone enum");
                    continue;
                }

                StandLocation location = _locationDictionary[baseZone];

                foreach (string zoneName in kvp.Value)
                {
                    if (!Enum.TryParse(zoneName, out GTZone additionalZone))
                    {
                        Plugin.Logger.LogWarning($"Text '{kvp.Key}' does not fit into GTZone enum");
                        continue;
                    }

                    _locationDictionary.TryAdd(additionalZone, location);
                }
            }

            OnZoneChange(ZoneManagement.instance.zones);
            ZoneManagement.OnZoneChange += OnZoneChange;
        }

        private async Task<string> GetContent(string path)
        {
#if DEBUG
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path.Replace('/', '.'));
            if (stream == null) return string.Empty;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
#else
            string url = $"https://raw.githubusercontent.com/developer9998/GorillaShirts/refs/heads/main/{path}";
            using UnityWebRequest request = UnityWebRequest.Get(url);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await operation;
            return request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text.Trim() : string.Empty;
#endif
        }

        public void OnZoneChange(ZoneData[] zoneData)
        {
            IEnumerable<GTZone> activeZones = zoneData.Where(zone => zone.active).Select(zone => zone.zone);
            OnZoneChange(activeZones.ToArray());
        }

        public void OnZoneChange(GTZone[] activeZones)
        {
            foreach (GTZone zone in activeZones)
            {
                if (_locationDictionary.TryGetValue(zone, out StandLocation location))
                {
                    MoveStand(location.Position, location.EulerAngles);
                    return;
                }
            }

            //Root.SetActive(false);
        }

        public void MoveStand(Transform transform) => MoveStand(transform.position, transform.eulerAngles);

        public void MoveStand(Vector3 position, Vector3 direction)
        {
            Root.transform.position = position;
            Root.transform.rotation = Quaternion.Euler(direction);
            //Root.SetActive(true);
        }

        public void SetMaterialState(bool useUberMaterials)
        {
            if (_useUberMaterials == useUberMaterials) return;

            _useUberMaterials = useUberMaterials;
            _standRenderers.ForEach(renderer => renderer.materials = _useUberMaterials ? _uberMaterials[renderer] : _baseMaterials[renderer]);
        }

        public void SetVisibility(StandVisibility culling)
        {
            if (Root == null) return;

            /*
            if (culling != ShirtStandCullLayer.None)
            {
                mainMenuRoot.GetComponentInParent<Canvas>(true).GetComponentsInChildren<Graphic>(true).Select(graphic => graphic.gameObject).ForEach(gameObject => gameObject.layer = culling == ShirtStandCullLayer.All ? (int)UnityLayer.UI : (int)ShirtStandCullLayer.FirstPerson);
                _standObjectLayers.ForEach(kvp => kvp.Key.layer = culling == ShirtStandCullLayer.All ? kvp.Value : (int)UnityLayer.FirstPersonOnly);
            }
            */

            bool active = culling != StandVisibility.None;
            if (Root.activeSelf != active) Root.SetActive(active);
        }

#endif
        }
}
