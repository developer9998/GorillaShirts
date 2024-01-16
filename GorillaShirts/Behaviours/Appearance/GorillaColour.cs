using UnityEngine;

namespace GorillaShirts.Behaviours.Visuals
{
    public class GorillaColour : MonoBehaviour
    {
        public VisualParent Ref_VisualParent;
        private VRRig Ref_Rig;

        private Material Ref_Material;

        private bool IsInit;
        private Color TargetColour = Color.clear, ActiveColour = Color.clear;

        public void Start() => Ref_Material = GetComponent<Renderer>().material;

        public void OnDisable() => IsInit = false;

        public void Update()
        {
            if (Ref_VisualParent.Rig == null) return;

            Ref_Rig ??= Ref_VisualParent.Rig.RigParent.GetComponent<VRRig>();
            TargetColour = Ref_Rig != null ? Ref_Rig.materialsToChangeTo[0].color == Color.white ? Ref_Rig.materialsToChangeTo[0].color : Ref_Rig.materialsToChangeTo[Ref_Rig.setMatIndex].color : Ref_VisualParent.Rig.RigSkin.material.color;

            if (!IsInit)
            {
                IsInit = true;
                ActiveColour = TargetColour;
            }

            float deltaTime = 15f * Time.deltaTime;
            ActiveColour = new(Mathf.Lerp(ActiveColour.r, TargetColour.r, deltaTime), Mathf.Lerp(ActiveColour.g, TargetColour.g, deltaTime), Mathf.Lerp(ActiveColour.b, TargetColour.b, deltaTime));
            Ref_Material.SetColor("_BaseColor", LightenColour(ActiveColour));
        }

        private Color LightenColour(Color original, float amount = 0.8f)
        {
            float oneMinus = 1 - amount;
            float r = original.r * (amount + oneMinus);
            float g = original.g * (amount + oneMinus);
            float b = original.b * (amount + oneMinus);
            return new Color(r, g, b, original.a);
        }
    }
}
