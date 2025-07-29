using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_GhostReactor : Location_Base
    {
        public override GTZone[] Zones => [GTZone.ghostReactor];
        public override Vector3 Position => new(-27.7124f, -26.9491f, -62.0407f);
        public override Vector3 EulerAngles => Vector3.up * 180f;
    }
}
