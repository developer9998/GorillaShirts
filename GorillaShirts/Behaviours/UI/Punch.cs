using UnityEngine;

#if PLUGIN
using GorillaExtensions;
using GorillaLocomotion;
using System.Linq;
#endif

namespace GorillaShirts.Behaviours.UI
{
    internal class Punch : MonoBehaviour
    {
        public AudioSource audioDevice;

        public Animator animator;

#if PLUGIN
        public string[] PunchAnimationNames = ["Punch1"];
#else
        public string[] PunchAnimationNames = new string[1] { "Punch1" };
#endif

#if PLUGIN

        private GorillaVelocityEstimator leftHandEstimator, rightHandEstimator;

        public void Awake()
        {
            leftHandEstimator = GTPlayer.Instance.leftControllerTransform.gameObject.GetOrAddComponent<GorillaVelocityEstimator>();
            rightHandEstimator = GTPlayer.Instance.rightControllerTransform.gameObject.GetOrAddComponent<GorillaVelocityEstimator>();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator handIndicator))
            {
                var velocityEstimator = handIndicator.isLeftHand ? leftHandEstimator : rightHandEstimator;
                if (velocityEstimator.linearVelocity.sqrMagnitude < (1f * 1f)) return;

                var clipInfoArray = animator.GetCurrentAnimatorClipInfo(0);
                if (clipInfoArray != null && clipInfoArray.Length > 0 && clipInfoArray[0].clip is var clip && (PunchAnimationNames.Contains(clip.name) || clip.name.StartsWith("Spawn"))) return;

                animator.Play(PunchAnimationNames[Random.Range(0, PunchAnimationNames.Length)]);
                audioDevice.Play();
                GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.tapHapticDuration / 1.25f);
            }
        }

#endif
    }
}
