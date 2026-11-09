using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_VirtualStump : Location_Base
    {
        public override GTZone[] Zones => [GTZone.customMaps];
        public override Vector3 Position => new(-1.0324f, -10.3759f, 0.219f);
        public override Vector3 EulerAngles => Vector3.up * 270.9822f;
    }
}
