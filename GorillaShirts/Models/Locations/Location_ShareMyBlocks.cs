using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_ShareMyBlocks : Location_Base
    {
        public override GTZone[] Zones => [GTZone.monkeBlocksShared];
        public override Vector3 Position => new(-280.5048f, 31.0107f, -219.9212f);
        public override Vector3 EulerAngles => Vector3.up * 236f;
    }
}
