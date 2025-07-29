using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_VirtualStump : Location_Base
    {
        public override GTZone[] Zones => [GTZone.customMaps];
        public override Vector3 Position => new(1.3714f, -10.4042f, -1.6228f);
        public override Vector3 EulerAngles => Vector3.up * 329.6566f;
    }
}
