using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class CanyonLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.canyon];
        public Vector3 Position => new(-100.246f, 17.9809f, -169.374f);
        public Vector3 EulerAngles => Vector3.up * 297.5797f;
    }
}
