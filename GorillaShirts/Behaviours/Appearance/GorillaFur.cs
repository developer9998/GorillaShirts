using UnityEngine;

namespace GorillaShirts.Behaviours.Visuals
{
    public class GorillaFur : MonoBehaviour
    {
        public VisualParent Ref_VisualParent;
        private VRRig Ref_Rig;

        private Renderer Ref_Renderer;

        private string Mode = "0";

        public void Start()
        {
            if (Ref_VisualParent.Rig == null) return;

            Ref_Renderer = GetComponent<Renderer>();
            Ref_Rig ??= Ref_VisualParent.Rig.RigParent.GetComponent<VRRig>();

            Mode = transform.GetChild(transform.childCount - 1).name[^1].ToString();
            switch (Mode)
            {
                case "0":
                    Ref_Renderer.material = Ref_Rig != null ? Ref_Rig.materialsToChangeTo[0] : Ref_VisualParent.Rig.RigSkin.material;
                    break;
                case "1":
                    Material material = new(Ref_Rig != null ? Ref_Rig.materialsToChangeTo[0] : Ref_VisualParent.Rig.RigSkin.material)
                    {
                        color = Color.white
                    };
                    Ref_Renderer.material = material;
                    break;
            }
        }


        public void Update()
        {
            if (Mode != "2") return;

            Ref_Rig ??= Ref_VisualParent.Rig.RigParent.GetComponent<VRRig>();
            Ref_Renderer.material = Ref_Rig != null ? Ref_Rig.mainSkin.material : Ref_VisualParent.Rig.RigSkin.material;
        }
    }
}
