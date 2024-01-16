using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaShirts.Interaction
{
    public class Punch : MonoBehaviour
    {
        private AudioSource _punchSource;
        private Animator _animator;

        private GorillaVelocityEstimator _leftVelocity, _rightVelocity;

        private const float _punchDebounce = 0.44f;
        private float _punchTimeLast;
        private bool _punchState;

        public void Start()
        {
            _punchSource = GetComponentInChildren<AudioSource>();
            _animator = GetComponent<Animator>();
            _animator.SetBool("Punch", false);

            _leftVelocity = Player.Instance.leftControllerTransform.gameObject.GetOrAddComponent<GorillaVelocityEstimator>();
            _rightVelocity = Player.Instance.rightControllerTransform.gameObject.GetOrAddComponent<GorillaVelocityEstimator>();

            gameObject.layer = (int)UnityLayer.GorillaInteractable;
            GetComponent<Collider>().isTrigger = true;
        }

        public void Update()
        {
            if (_punchState && Time.time > _punchTimeLast + _punchDebounce)
            {
                _animator.SetBool("Punch", false);
                _punchState = false;
            }
        }

        public bool PunchMethod()
        {
            if (_punchState) return false;
            _punchState = true;

            _animator.SetBool("Punch", true);
            _punchTimeLast = Time.time;
            return true;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component))
            {
                GorillaVelocityEstimator _velocityEstimator = component.isLeftHand ? _leftVelocity : _rightVelocity;
                if (_velocityEstimator.linearVelocity.magnitude < 2.4f) return;

                if (PunchMethod())
                {
                    _punchSource.Play();
                    GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.taggedHapticStrength / 1.1f, GorillaTagger.Instance.tapHapticDuration / 1.25f);
                }
            }
        }
    }
}
