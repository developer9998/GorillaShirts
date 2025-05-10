using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    public class BayouLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.bayou];
        public Vector3 Position => new(-123.1133f, -12.1695f, -91.0308f);
        public Vector3 EulerAngles => Vector3.up * 34.6322f;
    }
}
