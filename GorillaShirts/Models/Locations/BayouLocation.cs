using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    public class BayouLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.bayou];
        public Vector3 Position => new(-147.6207f, -15.2281f, -64.5603f);
        public Vector3 EulerAngles => Vector3.up * 34.6322f;
    }
}
