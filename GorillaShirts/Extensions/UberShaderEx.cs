using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GorillaShirts.Extensions
{
    public static class UberShaderEx
    {
        private static readonly string[] supportedShaderNames = ["Universal Render Pipeline/Unlit", "Universal Render Pipeline/Lit", "Unlit/Texture", "Custom/UnlitAO", "GorillaShirts/ColourTex", "GorillaShirts/UnlitOutline", "GorillaShirts/UnlitRGB"];

        private static readonly string[] supportedKeywords = ["_USE_TEXTURE", "_WATER_EFFECT", "_HEIGHT_BASED_WATER_EFFECT"];

        private static readonly string[] unsupportedKeywords = ["_USE_TEX_ARRAY_ATLAS"];

        private static string[] keywords = null;

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

        public static Material CreateUberMaterial(this Material baseMaterial)
        {
            Shader uberShader = UberShader.GetShader();

            if (baseMaterial == null || !baseMaterial || !supportedShaderNames.Contains(baseMaterial.shader.name) || baseMaterial.shader == uberShader)
                return baseMaterial;

            if (baseMaterial.shader.name == uberShader.name)
                return baseMaterial.ResolveUberMaterial();

            GetKeywords();

            Material uberMaterial = new(baseMaterial)
            {
                shader = uberShader
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

                if (hasTexture && hasColour) break;
            }

            uberMaterial.shaderKeywords = keywords;
            uberMaterial.enabledKeywords = [.. keywords.Select(keyword => new LocalKeyword(uberMaterial.shader, keyword))];

            return uberMaterial;
        }

        public static Material ResolveUberMaterial(this Material baseMaterial)
        {
            Shader uberShader = UberShader.GetShader(); // The latest UberShader, can be more up to date than what the baseMaterial uses

            if (baseMaterial == null || !baseMaterial || baseMaterial.shader.name != uberShader.name) return null;

            Material uberMaterial = new(baseMaterial)
            {
                shader = uberShader
            };

            return uberMaterial;
        }
    }
}
