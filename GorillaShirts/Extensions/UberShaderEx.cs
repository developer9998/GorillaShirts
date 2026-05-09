using GorillaShirts.Tools;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GorillaShirts.Extensions
{
    public static class UberShaderEx
    {
        private static readonly string[] supportedShaderNames =
        [
            // unity shaders
            "Universal Render Pipeline/Unlit",
            "Universal Render Pipeline/Lit",
            "Unlit/Texture",
            "Unlit/Color",
            // custom shaders
            "Custom/UnlitAO",
            "GorillaShirts/ColourTex",
            "GorillaShirts/UnlitRGB",
            "Shader Graphs/LitColorTex_Overlay",
            "Shader Graphs/UnlitColorTex"
        ];

        private static readonly string[] supportedKeywords =
        [
            "_USE_TEXTURE",
            "_WATER_EFFECT",
            "_HEIGHT_BASED_WATER_EFFECT"
        ];

        private static readonly string[] unsupportedKeywords =
        [
            "_USE_TEX_ARRAY_ATLAS"
        ];

        private static string[] keywords = null;

        private static void GetKeywords()
        {
            if (keywords is not null) return;

            if (GorillaTagger.Instance.offlineVRRig.myDefaultSkinMaterialInstance is Material material && material)
            {
                keywords = [.. material.shaderKeywords.Except(unsupportedKeywords)];
                return;
            }

            keywords = supportedKeywords;
        }

        public static Material CreateUberMaterial(this Material baseMaterial)
        {
            if (baseMaterial is null || !baseMaterial) throw new ArgumentNullException(nameof(baseMaterial));

            Shader uberShader = UberShader.GetShader();

            if (baseMaterial.shader == uberShader) return baseMaterial;

            if (!supportedShaderNames.Contains(baseMaterial.shader.name))
            {
                Logging.Warning($"CreateUberMaterial doesn't support shader: {baseMaterial.shader.name}");
                return baseMaterial;
            }

            GetKeywords();

            Material uberMaterial = new(baseMaterial)
            {
                shader = uberShader
            };

            bool hasTexture = false, hasColour = false;

            int propertyCount = baseMaterial.shader.GetPropertyCount();

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
    }
}
