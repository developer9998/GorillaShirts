using UnityEngine;

namespace GorillaShirts.Behaviors.Visuals
{
    public class GorillaFur : MonoBehaviour
    {
        public VisualParent _visualParent;
        private VRRig _actualPlayer;

        private Renderer _renderer;
        private Material _material;

        private string mode = "0";

        public void Start()
        {
            if (_visualParent.Rig == null) return;

            _renderer = GetComponent<Renderer>();
            _actualPlayer ??= _visualParent.Rig.RigParent.GetComponent<VRRig>();

            mode = name[^1].ToString();
            switch (mode)
            {
                case "0":
                    _renderer.material = _actualPlayer.materialsToChangeTo[0];
                    break;
                case "1":
                    _material = new Material(_actualPlayer.materialsToChangeTo[0]);
                    _material.color = Color.white;
                    _renderer.material = _material;
                    break;
            }
        }


        public void Update()
        {
            if (mode != "2") return;

            _actualPlayer ??= _visualParent.Rig.RigParent.GetComponent<VRRig>();
            _renderer.material = _actualPlayer.mainSkin.material;
        }
    }
}
