using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GorillaShirts.Extensions
{
    public static class UberShaderEx
    {
        private static readonly string[] supportedShaderNames = ["Universal Render Pipeline/Unlit", "Universal Render Pipeline/Lit", "Unlit/Texture", "Custom/UnlitAO"];

        private static string[] keywords = null;
        private static readonly string[] supportedKeywords = ["_USE_TEXTURE", "_WATER_EFFECT", "_HEIGHT_BASED_WATER_EFFECT", "_EMISSION"];
        private static readonly string[] unsupportedKeywords = ["_GT_BASE_MAP_ATLAS_SLICE_SOURCE__PROPERTY", "_USE_TEX_ARRAY_ATLAS"];

        private static void GetKeywords()
        {
            if (keywords is not null) return;

            if (VRRigCache.Instance.localRig is RigContainer localRig && localRig.Rig.myDefaultSkinMaterialInstance is Material material)
            {
                keywords = [.. material.shaderKeywords.Except(unsupportedKeywords)];
                return;
            }

            keywords = supportedKeywords;
        }

        public static Material CreateUberShaderVariant(this Material baseMaterial)
        {
            if (baseMaterial == null || !supportedShaderNames.Contains(baseMaterial.shader.name))
                return baseMaterial;

            GetKeywords();

            Material uberMaterial = new(baseMaterial)
            {
                shader = UberShader.GetShader()
            };

            int propertyCount = baseMaterial.shader.GetPropertyCount();
            bool hasTexture = false, hasColour = false;
            for (int i = 0; i < propertyCount; i++)
            {
                ShaderPropertyType propertyType = baseMaterial.shader.GetPropertyType(i);

                if (!hasTexture && propertyType == ShaderPropertyType.Texture)
                {
                    hasTexture = true;
                    int nameId = baseMaterial.shader.GetPropertyNameId(i);
                    uberMaterial.mainTexture = baseMaterial.GetTexture(nameId);
                    uberMaterial.mainTextureScale = baseMaterial.GetTextureScale(nameId);
                    uberMaterial.mainTextureOffset = baseMaterial.GetTextureOffset(nameId);
                }

                if (!hasColour && propertyType == ShaderPropertyType.Color)
                {
                    hasColour = true;
                    int nameId = baseMaterial.shader.GetPropertyNameId(i);
                    uberMaterial.color = baseMaterial.GetColor(nameId);
                }
            }

            uberMaterial.shaderKeywords = [.. keywords.Concat(baseMaterial.shaderKeywords).Distinct()];
            uberMaterial.enabledKeywords = [.. keywords.Select(keyword => new LocalKeyword(uberMaterial.shader, keyword))];

            return uberMaterial;
        }

        public static Material UpdateUberShaderMaterial(this Material baseMaterial)
        {
            if (baseMaterial == null || !baseMaterial || baseMaterial.shader.name != UberShader.GetShader().name) return null;

            Material uberMaterial = new(baseMaterial)
            {
                shader = UberShader.GetShader()
            };
            return uberMaterial;
        }
    }
}
