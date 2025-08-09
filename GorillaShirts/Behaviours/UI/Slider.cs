using UnityEngine;

#if PLUGIN
using HandIndicator = GorillaTriggerColliderHandIndicator;
#endif

namespace GorillaShirts.Behaviours.UI
{
    [RequireComponent(typeof(BoxCollider))]
    public class Slider : MonoBehaviour
    {
        public Transform Needle, StartPoint, EndPoint;

        public int Split = 100;

#if PLUGIN

        public float Value;

        private HandIndicator Current;

        public void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out HandIndicator component) && (Current == null || Current == component))
            {
                Vector3 local = transform.InverseTransformPoint(component.transform.position);
                float tbaValue = Mathf.RoundToInt(Mathf.Clamp01((local.x - StartPoint.localPosition.x) / (EndPoint.localPosition.x * 2f)) * Split) / (float)Split;
                Needle.transform.localPosition = GetNeedlePosition(tbaValue);

                if (tbaValue != Value)
                {
                    Value = tbaValue;
                    if (Current != null) GorillaTagger.Instance.StartVibration(component.isLeftHand, 0.1f, 0.015f);
                }

                if (Current == null)
                {
                    Current = component;
                    GorillaTagger.Instance.StartVibration(component.isLeftHand, 0.25f, 0.05f);
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out HandIndicator component) && Current == component)
            {
                Current = null;
            }
        }

        public void OnDisable()
        {
            if (Current is not null)
            {
                Current = null;
            }
        }

        public void SetValue(float value)
        {
            Value = value;
            Needle.transform.localPosition = GetNeedlePosition(value);
        }

        private Vector3 GetNeedlePosition(float value) => Vector3.Lerp(StartPoint.localPosition, EndPoint.localPosition, value);

#endif
    }
}
