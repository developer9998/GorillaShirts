using UnityEngine;

#if PLUGIN
using System.Collections.Generic;
using System.Linq;
using GorillaShirts.Models;
using GorillaShirts.Models.Locations;
using GorillaShirts.Extensions;
#endif

namespace GorillaShirts.Behaviours.UI
{
    public class Stand : MonoBehaviour
    {
        public GameObject Object;

        public AudioSource Audio;

        public Camera Camera;

        public MainMenu Display;

#if PLUGIN
        public StandRigHandler Rig; // TODO: define standrig in unity

        private List<Renderer> renderers;

        private Dictionary<Renderer, Material[]> original_materials;

        private Dictionary<Renderer, Material[]> uber_shader_materials;

        private bool uber_variants_used = false;

        private readonly List<IStandLocation> locations =
        [
            new ForestLocation(),
            new CaveLocation(),
            new CanyonLocation(),
            new CityLocation(),
            new MountainLocation(),
            new CloudsLocation(),
            new BasementLocation(),
            new BeachLocation(),
            new TutorialLocation(),
            new RotatingLocation(),
            new MetropolisLocation(),
            new ArcadeLocation(),
            new BayouLocation(),
            new VStumpLocation(),
            new AtriumLocation(),
            new MonkeBlockLocation(),
            new MinesLocation(),
            new MagmarenaLocation(),
            new HoverpackLocation(),
            new CrittersLocation(),
            new GhostReactorLocation()
        ];

        public void Awake()
        {
            ZoneManagement.OnZoneChange += OnZoneChange;

            renderers = [.. Object.GetComponentsInChildren<Renderer>(true)];

            original_materials = renderers.ToDictionary(renderer => renderer, renderer => renderer.materials);

            uber_shader_materials = renderers.ToDictionary(renderer => renderer, renderer => renderer.materials.Select(material => material.CreateUberShaderVariant()).ToArray());
        }

        public void OnZoneChange(ZoneData[] zones)
        {
            IEnumerable<GTZone> activeZones = zones.Where(zone => zone.active).Select(zone => zone.zone);
            OnZoneChange(activeZones.ToArray());
        }

        public void OnZoneChange(GTZone[] zones)
        {
            foreach (GTZone currentZone in zones)
            {
                IStandLocation currentLocation = locations.Find(location => location.Zones.Contains(currentZone));

                if (currentLocation != null)
                {
                    bool use_uber_variants = currentZone == GTZone.ghostReactor;
                    if (uber_variants_used != use_uber_variants)
                    {
                        uber_variants_used = use_uber_variants;
                        renderers.ForEach(renderer => renderer.materials = uber_variants_used ? uber_shader_materials[renderer] : original_materials[renderer]);
                    }
                    MoveStand(currentLocation.Position, currentLocation.EulerAngles);
                    return;
                }
            }

            Object.SetActive(false);
        }

        public void MoveStand(Transform transform) => MoveStand(transform.position, transform.eulerAngles);

        public void MoveStand(Vector3 position, Vector3 direction)
        {
            Object.transform.position = position;
            Object.transform.rotation = Quaternion.Euler(direction);
            Object.SetActive(true);
        }
#endif
    }
}
