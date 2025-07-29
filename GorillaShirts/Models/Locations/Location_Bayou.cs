using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Bayou : Location_Base
    {
        public override GTZone[] Zones => [GTZone.bayou];
        public override Vector3 Position => new(-147.866f, -15.2297f, -65.8597f);
        public override Vector3 EulerAngles => Vector3.up * 210f;
    }
}
