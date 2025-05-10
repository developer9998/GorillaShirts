using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class CrittersLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.critters];
        public Vector3 Position => new(96.8232f, -93.213f, 40.2771f);
        public Vector3 EulerAngles => Vector3.up * 335.8066f;
    }
}
