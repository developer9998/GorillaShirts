using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class Location_Arena : Location_Base
    {
        public override GTZone[] Zones => [GTZone.arena];
        public override Vector3 Position => new(100.5464f, 3.9509f, 196.2157f);
        public override Vector3 EulerAngles => Vector3.zero;
    }
}
