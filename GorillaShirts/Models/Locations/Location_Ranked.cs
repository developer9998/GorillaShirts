using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Ranked : Location_Base
    {
        public override GTZone[] Zones => [GTZone.ranked];
        public override Vector3 Position => new(-108.3281f, 17.9325f, -273.5047f);
        public override Vector3 EulerAngles => Vector3.up * 53.2568f;
    }
}
