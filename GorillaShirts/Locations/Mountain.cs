using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Mountain : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.mountain;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-19.6436f, 18.1985f, -108.6495f), new(0f, 5.7453f, 0f));
    }
}
