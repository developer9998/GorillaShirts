using System;
using GorillaShirts.Models;
using UnityEngine;

namespace GorillaShirts.Behaviours.UI
{
    [RequireComponent(typeof(BoxCollider), typeof(Renderer))]
    public class Button : MonoBehaviour
    {
        public const float Debounce = 0.25f;

        public EButtonType Type;
        public event Action<GorillaTriggerColliderHandIndicator> OnPress;

        private static float _lastPress;

        private Renderer meshRenderer;
        private Gradient gradient;
        private float timeStamp = 1;

        public void Start()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            gameObject.layer = (int)UnityLayer.GorillaInteractable;

            gradient = new Gradient();
            meshRenderer = GetComponent<Renderer>();

            var colourKeysH = new GradientColorKey[3];
            var alphaKeysH = new GradientAlphaKey[2];

            // Deal with alpha keys first, they're easier
            alphaKeysH[0] = new GradientAlphaKey(1f, 0f);
            alphaKeysH[1] = new GradientAlphaKey(1f, Debounce);

            // Now deal with the colours
            colourKeysH[0] = new GradientColorKey(new Color32(101, 101, 101, 255), 0);
            colourKeysH[1] = new GradientColorKey(new Color32(140, 140, 140, 255), Debounce / 3f);
            colourKeysH[2] = new GradientColorKey(new Color32(101, 101, 101, 255), Debounce);

            gradient.SetKeys(colourKeysH, alphaKeysH);
            timeStamp = Debounce;
        }

        public void Update()
        {
            if (meshRenderer == null) return;

            timeStamp += Time.unscaledDeltaTime;
            meshRenderer.material.color = timeStamp <= Debounce ? gradient.Evaluate(timeStamp) : new Color32(101, 101, 101, 255);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && Time.time > _lastPress + Debounce)
            {
                timeStamp = 0;
                _lastPress = Time.time;

                OnPress?.Invoke(component);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 1.25f, GorillaTagger.Instance.tapHapticDuration / 1.1f);
            }
        }
    }
}
