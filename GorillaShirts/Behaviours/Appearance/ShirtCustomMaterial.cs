using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    [RequireComponent(typeof(MeshRenderer)), DisallowMultipleComponent]
    [AddComponentMenu("GorillaShirts/Appearance/Custom Material")]
    public class ShirtCustomMaterial : MonoBehaviour
    {
        // public EMaterialSource Source;

        public EAppearanceType Appearance;

#if PLUGIN
        public int[] MaterialIndexes = [0];
#else
        public int[] MaterialIndexes = new int[1] { 0 };
#endif

#if PLUGIN

        public ShirtColourProfile ShirtProfile;

        public Material BaseFurMaterial;

        private Material material;

        private Renderer renderer;

        public void Start()
        {
            if (BaseFurMaterial)
            {
                material = new Material(BaseFurMaterial);
            }

            renderer = GetComponent<Renderer>();
            ApplyColour();
        }

        public void OnEnable()
        {
            ShirtProfile.OnColourChanged += ApplyColour;
        }

        public void OnDisable()
        {
            ShirtProfile.OnColourChanged -= ApplyColour;
        }

        public void ApplyColour()
        {
            if (material == null) return;

            switch (Appearance)
            {
                case EAppearanceType.SyncMaterial:
                    renderer.material = ShirtProfile.Rig ? ShirtProfile.Rig.mainSkin.material : ShirtProfile.Humanoid.MainSkin.material;
                    break;
                default:
                    renderer.material = material;
                    material.color = (Appearance == EAppearanceType.SyncColour && ShirtProfile.Rig) ? ShirtProfile.Colour : Color.white;
                    break;
            }
        }

#endif

        public enum EAppearanceType
        {
            Fur,
            SyncColour,
            SyncMaterial
        }
    }
}
