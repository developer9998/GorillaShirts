using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class RankedLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.ranked];
        public Vector3 Position => new(-108.1697f, 17.9511f, -273.5289f);
        public Vector3 EulerAngles => Vector3.up * 44.9243f;
    }
}
