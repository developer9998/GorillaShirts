using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Tutorial : Location_Base
    {
        public override GTZone[] Zones => [GTZone.tutorial];
        public override Vector3 Position => new(-98.9992f, 37.6046f, -72.6943f);
        public override Vector3 EulerAngles => Vector3.up * 8.7437f;
    }
}
