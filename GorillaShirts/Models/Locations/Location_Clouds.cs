using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Clouds : Location_Base
    {
        public override GTZone[] Zones => [GTZone.skyJungle];
        public override Vector3 Position => new(-76.7905f, 162.7874f, -100.4427f);
        public override Vector3 EulerAngles => Vector3.up * 342.6743f;
    }
}
