using UnityEngine;

#if PLUGIN
using GorillaExtensions;
using HandIndicator = GorillaTriggerColliderHandIndicator;
#endif

namespace GorillaShirts.Behaviours.UI
{
    [RequireComponent(typeof(BoxCollider))]
    public class ScrollableMenuController : MonoBehaviour
    {
        public Transform Needle, StartPoint, EndPoint;

        public Transform Container;
        public float StartCoord, EndCoord;

        public int Split = 100;

#if PLUGIN

        public float Value;

        private HandIndicator Current;

        public void Start()
        {
            UpdateContainer();
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out HandIndicator component) && (Current == null || Current == component))
            {
                Vector3 local = transform.InverseTransformPoint(component.transform.position);
                float tbaValue = Mathf.RoundToInt(Mathf.Clamp01((local.x - StartPoint.localPosition.x) / (EndPoint.localPosition.x * 2f)) * Split) / (float)Split;
                Needle.transform.localPosition = Vector3.Lerp(StartPoint.localPosition, EndPoint.localPosition, tbaValue);

                if (tbaValue != Value)
                {
                    Value = tbaValue;
                    UpdateContainer();
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
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && Current == component)
            {
                Current = null;
                UpdateContainer();
            }
        }


        public void UpdateContainer()
        {
            Container.transform.localPosition = Container.transform.localPosition.WithY(Mathf.Lerp(StartCoord, EndCoord, Value));
        }
#endif
    }
}
