using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class MagmarenaLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.arena];
        public Vector3 Position => new(100.5464f, 3.9509f, 196.2157f);
        public Vector3 EulerAngles => Vector3.zero;
    }
}
