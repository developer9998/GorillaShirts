using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Mountain : Location_Base
    {
        public override GTZone[] Zones => [GTZone.mountain];
        public override Vector3 Position => new(-21.1437f, 18.1985f, -104.7568f);
        public override Vector3 EulerAngles => Vector3.up * 5.7453f;
    }
}
