using GorillaShirts.Models.UI;
using UnityEngine;

namespace GorillaShirts.Behaviours.UI
{
    internal class PushButton : MonoBehaviour
    {
        public MeshRenderer Renderer;

        public EButtonType Type;

        public Color IdleColour = new Color32(101, 101, 101, 255);

        public Color PressedColour = new Color32(140, 140, 140, 255);

#if PLUGIN
        public const float Debounce = 0.25f;

        private static float _lastPress;

        private Gradient gradient;
        private float timeStamp = 1;

        private Material material;

        private bool updateButtonColour;

        public void Awake()
        {
            material = new Material(Renderer.material);
            Renderer.material = material;

            gradient = new Gradient();

            var colourKeysH = new GradientColorKey[3];
            var alphaKeysH = new GradientAlphaKey[2];

            // Deal with alpha keys first, they're easier
            alphaKeysH[0] = new GradientAlphaKey(1f, 0f);
            alphaKeysH[1] = new GradientAlphaKey(1f, Debounce);

            // Now deal with the colours
            colourKeysH[0] = new GradientColorKey(IdleColour, 0);
            colourKeysH[1] = new GradientColorKey(PressedColour, Debounce / 3f);
            colourKeysH[2] = new GradientColorKey(IdleColour, Debounce);

            gradient.SetKeys(colourKeysH, alphaKeysH);
            timeStamp = Debounce;

            material.color = gradient.Evaluate(timeStamp);
        }

        public void Update()
        {
            if (updateButtonColour)
            {
                timeStamp = Mathf.Min(Debounce, timeStamp += Time.unscaledDeltaTime);
                if (Mathf.Approximately(timeStamp, Debounce))
                {
                    material.color = IdleColour;
                    updateButtonColour = false;
                    return;
                }
                material.color = gradient.Evaluate(timeStamp);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && Time.realtimeSinceStartup > _lastPress + Debounce)
            {
                timeStamp = 0;
                _lastPress = Time.realtimeSinceStartup;
                updateButtonColour = true;

                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 1.5f, GorillaTagger.Instance.tapHapticDuration);

                if (Main.HasInstance)
                {
                    AudioSource audioDevice = component.isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
                    audioDevice.GTPlayOneShot(Main.Instance.Audio[Models.EAudioType.ButtonPress], 0.35f);
                    if (Main.Instance.MenuStateMachine.HasState) Main.Instance.MenuStateMachine.CurrentState.OnButtonPress(Type);
                }
            }
        }
#endif
    }
}