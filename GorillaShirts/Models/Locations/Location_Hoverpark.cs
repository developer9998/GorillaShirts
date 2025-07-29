using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Hoverpark : Location_Base
    {
        public override GTZone[] Zones => [GTZone.hoverboard];
        public override Vector3 Position => new(-88.5611f, -16.4928f, 28.6767f);
        public override Vector3 EulerAngles => Vector3.up * 302.9343f;
    }
}
