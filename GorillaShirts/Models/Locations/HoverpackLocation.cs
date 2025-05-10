using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class HoverpackLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.hoverboard];
        public Vector3 Position => new(-88.5611f, -16.4928f, 28.6767f);
        public Vector3 EulerAngles => Vector3.up * 302.9343f;
    }
}
