using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class RotatingLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.rotating];
        public Vector3 Position => new(-30.2096f, -10.256f, -341.8227f);
        public Vector3 EulerAngles => Vector3.up * 180f;
    }
}
