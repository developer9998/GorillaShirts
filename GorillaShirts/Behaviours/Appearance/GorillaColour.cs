using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public class GorillaColour : MonoBehaviour
    {
        public ShirtVisual ShirtVisual;

        private Material material;

        public void Start()
        {
            material = GetComponent<Renderer>().material;
            ApplyColour();
        }

        public void OnEnable()
        {
            ShirtVisual.OnColourChanged += ApplyColour;
        }

        public void OnDisable()
        {
            ShirtVisual.OnColourChanged -= ApplyColour;
        }

        public void ApplyColour()
        {
            Color colour = ShirtVisual.Colour;
            float minimumValue = 0.05f;

            Color.RGBToHSV(colour, out float h, out float s, out float v);
            v = Mathf.Clamp((v * (1 - minimumValue)) + minimumValue, minimumValue, 1f);
            material.SetColor("_BaseColor", Color.HSVToRGB(h, s, v));
        }
    }
}
