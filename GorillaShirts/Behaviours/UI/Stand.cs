using GorillaShirts.Behaviours.Appearance;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if PLUGIN
using GorillaShirts.Extensions;
using System;
using GorillaShirts.Models.Locations;
using System.Collections.Generic;
using System.Linq;
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

        private bool _isStandVisible = true;

        private readonly Dictionary<GTZone, Location_Base> _locationDictionary = [];

        private bool _useUberMaterials = false;

        private Renderer[] _standRenderers;

        private Dictionary<Renderer, Material[]> _baseMaterials, _uberMaterials;

        public void Start()
        {
            Type baseType = typeof(Location_Base);
            Type[] typeArray = baseType.Assembly.GetTypes();

            foreach (Type type in typeArray)
            {
                if (type.IsSubclassOf(baseType))
                {
                    Location_Base location = (Location_Base)Activator.CreateInstance(type);
                    location.Zones.Where(zone => !_locationDictionary.ContainsKey(zone)).ForEach(zone => _locationDictionary.Add(zone, location));
                }
            }

            _standRenderers = Root.GetComponentsInChildren<Renderer>(true);
            _baseMaterials = _standRenderers.ToDictionary(renderer => renderer, renderer => renderer.materials);
            _uberMaterials = _standRenderers.ToDictionary(renderer => renderer, renderer => renderer.materials.Select(material => material.CreateUberMaterial()).ToArray());
            SetMaterialState(Shader.IsKeywordEnabled("_ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX"));

            SetVisibility(Plugin.State);
            Plugin.OnStateChanged += SetVisibility;
      
            OnZoneChange(ZoneManagement.instance.zones);
            ZoneManagement.OnZoneChange += OnZoneChange;
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
                if (_locationDictionary.TryGetValue(zone, out Location_Base location))
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

        public void SetVisibility(bool isVisible)
        {
            if (_isStandVisible == isVisible) return;

            _isStandVisible = isVisible;
            Root.SetActive(_isStandVisible);
        }

#endif
    }
}
