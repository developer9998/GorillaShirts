using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class MinesLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.mines];
        public Vector3 Position => new(-45.7829f, -7.323f, -76.7468f);
        public Vector3 EulerAngles => Vector3.up * 53.7666f;
    }
}
