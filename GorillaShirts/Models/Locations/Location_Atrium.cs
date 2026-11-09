using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Atrium : Location_Base
    {
        public override GTZone[] Zones => [GTZone.mall];
        public override Vector3 Position => new(-53.9626f, -1.2215f, -113.6837f);
        public override Vector3 EulerAngles => Vector3.up * 214.5495f;
    }
}
