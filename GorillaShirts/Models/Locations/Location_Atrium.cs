using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Atrium : Location_Base
    {
        public override GTZone[] Zones => [GTZone.mall];
        public override Vector3 Position => new(-114.6934f, 16.9925f, -212.6736f);
        public override Vector3 EulerAngles => Vector3.up * 8.6951f;
    }
}
