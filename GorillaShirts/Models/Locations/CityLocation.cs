using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class CityLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.city, GTZone.cityNoBuildings];
        public Vector3 Position => new(-51.0506f, 16.7854f, -101.2362f);
        public Vector3 EulerAngles => Vector3.up * 31.8728f;
    }
}
