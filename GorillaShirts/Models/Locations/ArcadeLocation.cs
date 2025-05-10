using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class ArcadeLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.arcade];
        public Vector3 Position => new(-33.8662f, 22.0807f, -89.0348f);
        public Vector3 EulerAngles => Vector3.up * 119.5951f;
    }
}
