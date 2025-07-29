using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Rotating : Location_Base
    {
        public override GTZone[] Zones => [GTZone.rotating];
        public override Vector3 Position => new(-30.2096f, -10.256f, -341.8227f);
        public override Vector3 EulerAngles => Vector3.up * 180f;
    }
}
