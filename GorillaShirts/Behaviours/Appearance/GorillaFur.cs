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

            switch (furMode)
            {
                case FurMode.Match:
                    renderer.material = ShirtVisual.PlayerRig ? ShirtVisual.PlayerRig.mainSkin.material : ShirtVisual.Rig.RigSkin.material;
                    break;
                default:
                    renderer.material = material;
                    material.color = (furMode == FurMode.Coloured && ShirtVisual.PlayerRig) ? ShirtVisual.Colour : Color.white;
                    break;
            }
        }
    }
}
