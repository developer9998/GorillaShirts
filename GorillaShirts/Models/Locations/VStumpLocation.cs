using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class VStumpLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.customMaps];
        public Vector3 Position => new(1.3714f, -10.4042f, -1.6228f);
        public Vector3 EulerAngles => Vector3.up * 329.6566f;
    }
}
