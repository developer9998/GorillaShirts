using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Cave : Location_Base
    {
        public override GTZone[] Zones => [GTZone.cave];
        public override Vector3 Position => new(-76.981f, -19.429f, -29.6408f);
        public override Vector3 EulerAngles => Vector3.up * 92.0393f;
    }
}
