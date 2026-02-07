using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Metro : Location_Base
    {
        public override GTZone[] Zones => [GTZone.Metropolis];
        public override Vector3 Position => new(62.9778f, 4.0927f, -238.5725f);
        public override Vector3 EulerAngles => Vector3.up * 142.0577f;
    }
}
