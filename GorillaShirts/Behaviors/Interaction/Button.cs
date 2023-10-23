using System;
using UnityEngine;

namespace GorillaShirts.Behaviors.Interaction
{
    public class Button : MonoBehaviour
    {
        public const float _debounce = 0.25f;
        private float _lastPress;

        private Renderer _renderer;
        private Gradient _gradient;
        private float _timeStamp = 1;

        public ButtonType btnType;
        public event Action<GorillaTriggerColliderHandIndicator> btnAction;

        public void Start()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("GorillaInteractable");

            _gradient = new Gradient();
            _renderer = GetComponent<Renderer>();

            var colourKeysH = new GradientColorKey[3];
            var alphaKeysH = new GradientAlphaKey[2];

            // Deal with alpha keys first, they're easier
            alphaKeysH[0] = new GradientAlphaKey(1f, 0f);
            alphaKeysH[1] = new GradientAlphaKey(1f, _debounce);

            // Now deal with the colours
            colourKeysH[0] = new GradientColorKey(new Color32(101, 101, 101, 255), 0);
            colourKeysH[1] = new GradientColorKey(new Color32(140, 140, 140, 255), _debounce / 2f);
            colourKeysH[2] = new GradientColorKey(new Color32(101, 101, 101, 255), _debounce);

            _gradient.SetKeys(colourKeysH, alphaKeysH);
            _timeStamp = _debounce;
        }

        public void Update()
        {
            _timeStamp += Time.unscaledDeltaTime;
            _timeStamp = Mathf.Clamp(_timeStamp, 0, _debounce);
        }

        public void FixedUpdate() => _renderer.material.color = _gradient.Evaluate(_timeStamp);

        public void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) || _lastPress + _debounce > Time.unscaledTime)
                return;

            _timeStamp = 0;
            _lastPress = Time.unscaledTime;

            btnAction?.Invoke(component);
            GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
        }

        public static ButtonType GetButtonType(string name) => (ButtonType)Enum.Parse(typeof(ButtonType), name);
    }
}
