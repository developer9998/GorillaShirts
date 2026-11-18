using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Hoverpark : Location_Base
    {
        public override GTZone[] Zones => [GTZone.hoverboard];
        public override Vector3 Position => new(-85.8319f, -16.67f, 27.6693f);
        public override Vector3 EulerAngles => Vector3.up * 54.4972f;
    }
}
