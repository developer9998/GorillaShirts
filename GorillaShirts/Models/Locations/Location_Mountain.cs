using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Mountain : Location_Base
    {
        public override GTZone[] Zones => [GTZone.mountain];
        public override Vector3 Position => new(-19.6436f, 18.1985f, -108.6495f);
        public override Vector3 EulerAngles => Vector3.up * 5.7453f;
    }
}
