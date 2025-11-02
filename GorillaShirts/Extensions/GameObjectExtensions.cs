using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Tools;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GorillaShirts.Extensions
{
    internal static class GameObjectExtensions
    {
        private static readonly List<Type> allowedTypeList =
        [
            typeof(MeshRenderer),
            typeof(Transform),
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            typeof(Light),
            typeof(UniversalAdditionalLightData),
            typeof(ReflectionProbe),
            typeof(AudioSource),
            typeof(Animator),
            typeof(Animation),
            typeof(TextMesh),
            typeof(ParticleSystem),
            typeof(ParticleSystemRenderer),
            typeof(RectTransform),
            typeof(SpriteRenderer),
            typeof(BillboardRenderer),
            typeof(Canvas),
            typeof(CanvasRenderer),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(TrailRenderer),
            typeof(Camera),
            typeof(UniversalAdditionalCameraData),
            typeof(Text),
            typeof(TMP_Text),
            typeof(TextMeshPro),
            typeof(TextMeshProUGUI),
            typeof(ShirtDescriptor),
            typeof(ShirtCustomColour),
            typeof(ShirtCustomMaterial),
            typeof(ShirtWobbleRoot),
            typeof(ShirtBillboard)
        ];

        public static bool sanitizeFPLODs = true;

        public static void SanitizeRecursive(this GameObject gameObject)
        {
            Sanitize(gameObject);
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                SanitizeRecursive(child);
            }
        }

        public static void Sanitize(this GameObject gameObject)
        {
            if (gameObject == null || !gameObject) return;

            Component[] components = gameObject.GetComponents<Component>();

            for (int i = components.Length - 1; i >= 0; i--)
            {
                Type type = components[i].GetType();
                if (type == typeof(LODGroup))
                {
                    bool allowLOD = true;

                    LODGroup lodGroup = (LODGroup)components[i];
                    var lodArray = lodGroup.GetLODs();

                    for (int j = 0; j < lodArray.Length; j++)
                    {
                        var lod = lodArray[j];
                        if (lod.renderers.Length == 0)
                        {
                            allowLOD = !sanitizeFPLODs;
                            //lodGroup.enabled = false;
                            break;
                        }
                    }

                    if (allowLOD)
                    {
                        Logging.Info($"LODGroup for {gameObject.name} allowed");
                        continue;
                    }

                    Logging.Warning($"LODGroup for {gameObject.name} not allowed (used for first person)");
                    Object.Destroy(lodGroup);
                    continue;
                }

                if (allowedTypeList.Contains(type)) continue;
                Logging.Warning($"Component {gameObject.name} not allowed: {type.Name}");
                Object.Destroy(components[i]);
            }
        }
    }
}
