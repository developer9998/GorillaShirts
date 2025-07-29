using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Forest : Location_Base
    {
        public override GTZone[] Zones => [GTZone.forest, GTZone.cityWithSkyJungle];
        public override Vector3 Position => new(-64.0157f, 12.51f, -83.8341f);
        public override Vector3 EulerAngles => Vector3.up * 25.7659f;
    }
}
