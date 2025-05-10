using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class BeachLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.beach];
        public Vector3 Position => new(-13.4184f, 28.6591f, -25.4867f);
        public Vector3 EulerAngles => Vector3.up * 295.2179f;
    }
}
