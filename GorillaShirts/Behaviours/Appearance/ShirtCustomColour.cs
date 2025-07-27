using UnityEngine;

#if PLUGIN
//using GorillaExtensions;
using GorillaShirts.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
#endif

namespace GorillaShirts.Behaviours.Appearance
{
    [RequireComponent(typeof(MeshRenderer)), DisallowMultipleComponent]
    [AddComponentMenu("GorillaShirts/Appearance/Custom Colour")]
    public class ShirtCustomColour : MonoBehaviour
    {
#if PLUGIN
        public int[] MaterialIndexes = [0];
#else
        public int[] MaterialIndexes = new int[1] { 0 };
#endif

        public bool ApplyValueChanges = true;

#if PLUGIN

        [SerializeField]
        public ShirtColourProfile ShirtProfile;

        private List<Material> materials;

        private List<string> colourPropertyNames;

        public void Start()
        {
            //Logging.Message("ShirtCustomColour (Start)");
            //Logging.Info(transform.GetPath());

            materials = [];
            colourPropertyNames = [];

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer.materials != null && renderer.materials.Length > 0)
            {
                if (MaterialIndexes == null || MaterialIndexes.Length == 0)
                {
                    MaterialIndexes = [.. Enumerable.Range(0, MaterialIndexes.Length)];
                }

                Material[] providedMaterialArray = renderer.materials;

                for (int i = 0; i < MaterialIndexes.Length; i++)
                {
                    int index = MaterialIndexes[i];

                    if (providedMaterialArray.ElementAtOrDefault(index) is Material material && material)
                    {
                        material = new Material(material);

                        materials.Add(material);

                        int propertyCount = material.shader.GetPropertyCount();
                        string colourProperty = null;

                        for (int k = 0; k < propertyCount; k++)
                        {
                            ShaderPropertyType shaderPropertyType = material.shader.GetPropertyType(k);
                            if (shaderPropertyType == ShaderPropertyType.Color)
                            {
                                string propertyName = material.shader.GetPropertyName(k);
                                //Logging.Message($"ShirtCustomColour identified property name");
                                //Logging.Info($"{material.name}: {propertyName}");
                                colourProperty = propertyName;
                                break;
                            }
                        }

                        colourPropertyNames.Add(colourProperty ?? "_BaseColor");

                        providedMaterialArray[index] = material;
                    }
                }

                renderer.materials = providedMaterialArray;
            }

            ShirtProfile.AddRecipient(this, ApplyColour);
        }

        public void ApplyColour(Color colour)
        {
            if (materials == null || materials.Count == 0) return;

            if (ApplyValueChanges)
            {
                float minimumValue = 0.1f;
                Color.RGBToHSV(colour, out float H, out float S, out float V);
                V = Mathf.Clamp((V * (1f - minimumValue)) + minimumValue, minimumValue, 1f);
                colour = Color.HSVToRGB(H, S, V);
            }

            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetColor(colourPropertyNames[i], colour);
            }
        }
#endif
    }
}
