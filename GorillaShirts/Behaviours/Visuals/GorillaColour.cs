using UnityEngine;

namespace GorillaShirts.Behaviors.Visuals
{
    public class GorillaColour : MonoBehaviour
    {
        public VisualParent _visualParent;
        private VRRig _actualPlayer;
        private Material _material;

        private bool _colourInitialized;
        private Color _targetColour = Color.clear;
        private Color _currentColour = Color.clear;

        public void Start() => _material = GetComponent<Renderer>().material;

        public void OnDisable() => _colourInitialized = false;

        public void Update()
        {
            if (_visualParent.Rig == null) return;
            _actualPlayer ??= _visualParent.Rig.RigParent.GetComponent<VRRig>();
            _targetColour = _actualPlayer != null ? (_actualPlayer.materialsToChangeTo[_actualPlayer.setMatIndex].color == Color.white ? _actualPlayer.materialsToChangeTo[0].color : _actualPlayer.materialsToChangeTo[_actualPlayer.setMatIndex].color) : _visualParent.Rig.RigSkin.material.color;

            if (!_colourInitialized)
            {
                _colourInitialized = true;
                _currentColour = _targetColour;
            }

            float deltaTime = 15f * Time.deltaTime;
            _currentColour = new(Mathf.Lerp(_currentColour.r, _targetColour.r, deltaTime), Mathf.Lerp(_currentColour.g, _targetColour.g, deltaTime), Mathf.Lerp(_currentColour.b, _targetColour.b, deltaTime));
            _material.SetColor("_BaseColor", BetterColour(_currentColour));
        }

        private Color BetterColour(Color original)
        {
            float r = original.r * 0.75f + 0.25f;
            float g = original.g * 0.75f + 0.25f;
            float b = original.b * 0.75f + 0.25f;
            return new Color(r, g, b, original.a);
        }
    }
}
