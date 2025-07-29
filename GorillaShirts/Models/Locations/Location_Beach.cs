using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Beach : Location_Base
    {
        public override GTZone[] Zones => [GTZone.beach];
        public override Vector3 Position => new(-13.4184f, 28.6591f, -25.4867f);
        public override Vector3 EulerAngles => Vector3.up * 295.2179f;
    }
}
