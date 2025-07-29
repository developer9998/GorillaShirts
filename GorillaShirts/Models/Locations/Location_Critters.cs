using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Critters : Location_Base
    {
        public override GTZone[] Zones => [GTZone.critters];
        public override Vector3 Position => new(96.8232f, -93.213f, 40.2771f);
        public override Vector3 EulerAngles => Vector3.up * 335.8066f;
    }
}
