using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    public class GhostReactorLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.ghostReactor];
        public Vector3 Position => new(-27.7124f, -26.9491f, -62.0407f);
        public Vector3 EulerAngles => Vector3.up * 180f;
    }
}
