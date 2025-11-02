using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_GhostReactorDrill : Location_Base
    {
        public override GTZone[] Zones => [GTZone.ghostReactorDrill];
        public override Vector3 Position => new(-37.0309f, -51.3765f, -88.4297f);
        public override Vector3 EulerAngles => Vector3.up * 18.5455f;
    }
}
