using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class SharedMonkeBlockLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.monkeBlocksShared];
        public Vector3 Position => new(-280.1623f, 31.0132f, -220.6715f);
        public Vector3 EulerAngles => Vector3.up * 249.3985f;
    }
}
