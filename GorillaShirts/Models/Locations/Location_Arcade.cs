using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Arcade : Location_Base
    {
        public override GTZone[] Zones => [GTZone.arcade];
        public override Vector3 Position => new(-36.0703f, 14.4027f, -94.4919f);
        public override Vector3 EulerAngles => Vector3.up * 11.5272f;
    }
}
