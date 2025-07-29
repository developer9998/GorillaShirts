using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_City : Location_Base
    {
        public override GTZone[] Zones => [GTZone.city, GTZone.cityNoBuildings];
        public override Vector3 Position => new(-51.0506f, 16.7854f, -101.2362f);
        public override Vector3 EulerAngles => Vector3.up * 31.8728f;
    }
}
