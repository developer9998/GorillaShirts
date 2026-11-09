using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_GhostReactor : Location_Base
    {
        public override GTZone[] Zones => [GTZone.ghostReactor, GTZone.ghostReactorTunnel];
        public override Vector3 Position => new(-38.1279f, -25.3724f, -42.4211f);
        public override Vector3 EulerAngles => Vector3.up * 180f;
    }
}
