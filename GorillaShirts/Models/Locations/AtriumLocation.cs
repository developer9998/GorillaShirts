using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class AtriumLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.mall];
        public Vector3 Position => new(-114.6934f, 16.9925f, -212.6736f);
        public Vector3 EulerAngles => Vector3.up * 8.6951f;
    }
}
