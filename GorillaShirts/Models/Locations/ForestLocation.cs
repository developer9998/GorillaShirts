using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class ForestLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.forest, GTZone.cityWithSkyJungle];
        public Vector3 Position => new(-64.0157f, 12.51f, -83.8341f);
        public Vector3 EulerAngles => Vector3.up * 25.7659f;
    }
}
