using System;
using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public enum FurMode
    {
        Default, Coloured, Match
    }

    public class GorillaFur : MonoBehaviour
    {
        public bool IsThisWorkingImReallyNotSureAsToWhyItWouldnt => material != null;

        public ShirtVisual ShirtVisual;

        public Material BaseFurMaterial;

        private FurMode furMode;

        private Material material;

        private Renderer renderer;

        public void Start()
        {
            furMode = (FurMode)Convert.ToInt32(transform.GetChild(transform.childCount - 1).name[^1]);
            if (BaseFurMaterial)
            {
                material = new Material(BaseFurMaterial);
            }
            renderer = GetComponent<Renderer>();
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
            if (material == null) return;

            renderer.material = ShirtVisual.PlayerRig ? (furMode == FurMode.Match ? ShirtVisual.PlayerRig.mainSkin.material : material) : ShirtVisual.Rig.RigSkin.material;

            switch (furMode)
            {
                case FurMode.Default:
                    material.color = ShirtVisual.PlayerRig ? ShirtVisual.Colour : Color.white;
                    break;
                case FurMode.Coloured:
                    renderer.material.color = Color.white;
                    break;
            }
        }
    }
}
