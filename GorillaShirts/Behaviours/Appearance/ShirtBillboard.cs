using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public class ShirtBillboard : MonoBehaviour
    {
        public BillboardAxis Axis = BillboardAxis.VerticalOnly;

        public enum BillboardAxis
        {
            All,
            VerticalOnly
        }

#if PLUGIN

        public void Update()
        {
            Vector3 forward = Camera.main.transform.position - transform.position;
            Vector3 eulerAngles = Quaternion.LookRotation(forward, Vector3.up).eulerAngles;

            if (Axis == BillboardAxis.VerticalOnly)
            {
                eulerAngles.x = 0;
                eulerAngles.z = 0;
            }

            transform.eulerAngles = eulerAngles;
        }

#endif
    }
}
