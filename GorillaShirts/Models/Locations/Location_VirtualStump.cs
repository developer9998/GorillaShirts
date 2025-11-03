using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_VirtualStump : Location_Base
    {
        public override GTZone[] Zones => [GTZone.customMaps];
        public override Vector3 Position => new(2.1085f, -10.3759f, -0.881f);
        public override Vector3 EulerAngles => Vector3.up * 297.5811f;
    }
}
