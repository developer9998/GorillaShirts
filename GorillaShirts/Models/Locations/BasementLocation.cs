using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class BasementLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.basement];
        public Vector3 Position => new(-36.0703f, 14.4027f, -94.4919f);
        public Vector3 EulerAngles => Vector3.up * 11.5272f;
    }
}
