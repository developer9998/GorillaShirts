using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class CloudsLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.skyJungle];
        public Vector3 Position => new(-76.7905f, 162.7874f, -100.4427f);
        public Vector3 EulerAngles => Vector3.up * 342.6743f;
    }
}
