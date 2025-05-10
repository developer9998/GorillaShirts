using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GorillaShirts.Extensions
{
    public static class MaterialEx
    {
        private static string[] keywords = null;

        private static readonly string[] allowed_shaders = ["Universal Render Pipeline/Unlit", "Universal Render Pipeline/Lit", "Unlit/Texture", "Custom/UnlitAO"];

        private static void GetKeywords()
        {
            if (keywords != null)
                return;

            keywords = (GorillaTagger.Instance.offlineVRRig && GorillaTagger.Instance.offlineVRRig.myDefaultSkinMaterialInstance)
                ? GorillaTagger.Instance.offlineVRRig.myDefaultSkinMaterialInstance.shaderKeywords
                : [
                    "_USE_TEXTURE",
                    "_WATER_EFFECT",
                    "_HEIGHT_BASED_WATER_EFFECT",
                    "_EMISSION"
                ];

            keywords = [.. keywords.Except(["_GT_BASE_MAP_ATLAS_SLICE_SOURCE__PROPERTY", "_USE_TEX_ARRAY_ATLAS"])];
        }

        public static Material CreateUberShaderVariant(this Material baseMaterial)
        {
            GetKeywords();

            if (allowed_shaders.Contains(baseMaterial.shader.name))
            {
                var uberMaterial = new Material(baseMaterial);
                uberMaterial.shader = UberShader.GetShader();

                IEnumerable<int> propertyIndicies = Enumerable.Range(0, baseMaterial.shader.GetPropertyCount());
                if (propertyIndicies.Any(index => baseMaterial.shader.GetPropertyType(index) == ShaderPropertyType.Texture))
                {
                    uberMaterial.mainTexture = baseMaterial.mainTexture;
                    uberMaterial.mainTextureScale = baseMaterial.mainTextureScale;
                    uberMaterial.mainTextureOffset = baseMaterial.mainTextureOffset;
                }
                if (propertyIndicies.Any(index => baseMaterial.shader.GetPropertyType(index) == ShaderPropertyType.Color))
                {
                    uberMaterial.color = baseMaterial.color;
                }
                
                uberMaterial.shaderKeywords = [.. keywords, .. baseMaterial.shaderKeywords];
                uberMaterial.enabledKeywords = [.. keywords.Select(keyword => new LocalKeyword(uberMaterial.shader, keyword))];

                return uberMaterial;
            }

            return baseMaterial;
        }
    }
}
