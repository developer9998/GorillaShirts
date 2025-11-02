using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_GhostReactor : Location_Base
    {
        public override GTZone[] Zones => [GTZone.ghostReactor, GTZone.ghostReactorTunnel];
        public override Vector3 Position => new(-31.4569f, -25.3724f, -48.2227f);
        public override Vector3 EulerAngles => Vector3.up * 221.8183f;
    }
}
