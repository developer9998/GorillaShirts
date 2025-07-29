using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Canyon : Location_Base
    {
        public override GTZone[] Zones => [GTZone.canyon];
        public override Vector3 Position => new(-100.246f, 17.9809f, -169.374f);
        public override Vector3 EulerAngles => Vector3.up * 297.5797f;
    }
}
