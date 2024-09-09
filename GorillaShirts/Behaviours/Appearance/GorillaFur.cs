using UnityEngine;

namespace GorillaShirts.Behaviours.Visuals
{
    public class GorillaFur : MonoBehaviour
    {
        public VRRig Rig
        {
            get
            {
                _rig ??= Fur_VisualParent.Rig.RigParent.GetComponent<VRRig>();
                return _rig;
            }
        }
        private VRRig _rig;

        public ShirtVisual Fur_VisualParent;
        private string _furMode = "0";

        public Material BaseFurMaterial;
        private Material _localMaterial;

        private Renderer _renderer;

        public void Start()
        {
            if (Fur_VisualParent.Rig == null) return;

            _localMaterial = new Material(BaseFurMaterial);

            _renderer = GetComponent<Renderer>();
            _furMode = transform.GetChild(transform.childCount - 1).name[^1].ToString();
            switch (_furMode)
            {
                case "0":
                    _renderer.material = Rig != null ? _localMaterial : Fur_VisualParent.Rig.RigSkin.material;
                    _localMaterial.color = Rig != null ? Rig.playerColor : Color.white;
                    break;
                case "1":
                    _renderer.material = Rig != null ? _localMaterial : Fur_VisualParent.Rig.RigSkin.material;
                    _renderer.material.color = Color.white;
                    break;
            }
        }


        public void Update()
        {
            if (_furMode != "2") return;

            _renderer.material = Rig != null ? Rig.mainSkin.material : Fur_VisualParent.Rig.RigSkin.material;
        }
    }
}
