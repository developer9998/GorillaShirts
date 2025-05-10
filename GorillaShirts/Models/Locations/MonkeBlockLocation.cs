using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class MonkeBlockLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.monkeBlocks];
        public Vector3 Position => new(-123.7533f, 16.8881f, -219.073f);
        public Vector3 EulerAngles => Vector3.up * 209.9129f;
    }
}
