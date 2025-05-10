using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public class PlayerMaterialAppearance : MonoBehaviour
    {
        public EMaterialSource Source;

        public EAppearanceType Appearance;

#if PLUGIN

        public ShirtVisual ShirtVisual;

        public Material BaseFurMaterial;

        private Material material;

        private Renderer renderer;

        public void Start()
        {
            //Appearance = (EAppearanceType)Convert.ToInt32(transform.GetChild(transform.childCount - 1).name[^1]);

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

            switch (Appearance)
            {
                case EAppearanceType.Sync:
                    renderer.material = ShirtVisual.PlayerRig ? ShirtVisual.PlayerRig.mainSkin.material : ShirtVisual.RigHandler.MainSkin.material;
                    break;
                default:
                    renderer.material = material;
                    material.color = (Appearance == EAppearanceType.WithColour && ShirtVisual.PlayerRig) ? ShirtVisual.Colour : Color.white;
                    break;
            }
        }

#endif

        public enum EAppearanceType
        {
            Default,
            WithColour,
            Sync
        }

        public enum EMaterialSource
        {
            Skin,
            Chest
        }
    }
}
