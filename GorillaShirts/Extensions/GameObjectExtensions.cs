using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GorillaShirts.Extensions
{
    internal static class GameObjectExtensions
    {
        private static readonly List<Type> allowedTypeList =
        [
            typeof(Transform),
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            typeof(Light),
            typeof(ReflectionProbe),
            typeof(AudioSource),
            typeof(Animator),
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
            typeof(Text),
            typeof(ShirtDescriptor),
            typeof(ShirtCustomColour),
            typeof(ShirtCustomMaterial),
            typeof(ShirtWobbleRoot),
            typeof(ShirtBillboard)
        ];

        public static void SanitizeObjectRecursive(this GameObject gameObject)
        {
            SanitizeObject(gameObject);
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                if (child != null && child)
                {
                    SanitizeObjectRecursive(child);
                }
            }
        }

        public static void SanitizeObject(this GameObject gameObject)
        {
            if (gameObject == null || !gameObject) return;

            Component[] components = gameObject.GetComponents<Component>();

            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (allowedTypeList.Contains(components[i].GetType())) continue;
                Logging.Warning("Found disallowed component");
                Logging.Info(components[i].GetType().Name);
                Object.DestroyImmediate(components[i]);
            }
        }
    }
}
