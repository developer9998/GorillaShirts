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
    }
}
