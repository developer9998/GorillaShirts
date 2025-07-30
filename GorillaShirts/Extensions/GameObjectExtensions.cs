﻿using GorillaShirts.Behaviours.Appearance;
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

        public static void SanitizeObjectRecursive(this GameObject gameObject)
        {
            SanitizeObject(gameObject);
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                SanitizeObjectRecursive(child);
            }
        }

        public static void SanitizeObject(this GameObject gameObject)
        {
            if (gameObject == null || !gameObject) return;

            Component[] components = gameObject.GetComponents<Component>();

            for (int i = components.Length - 1; i >= 0; i--)
            {
                Type type = components[i].GetType();
                Logging.Info(type.Name);
                if (allowedTypeList.Contains(type)) continue;
                Logging.Warning("Component is not allowed");
                Object.Destroy(components[i]);
            }
        }
    }
}
