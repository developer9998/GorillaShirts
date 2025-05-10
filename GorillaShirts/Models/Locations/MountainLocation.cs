using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class MountainLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.mountain];
        public Vector3 Position => new(-19.6436f, 18.1985f, -108.6495f);
        public Vector3 EulerAngles => Vector3.up * 5.7453f;
    }
}
