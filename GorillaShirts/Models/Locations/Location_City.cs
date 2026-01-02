using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_City : Location_Base
    {
        public override GTZone[] Zones => [GTZone.city, GTZone.cityNoBuildings];
        public override Vector3 Position => new(-46.5689f, 16.9125f, -102.7796f);
        public override Vector3 EulerAngles => Vector3.up * 1.9819f;
    }
}
