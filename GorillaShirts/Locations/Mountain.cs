using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Mountain : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.mountain;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-17.8902f, 18.1936f, -106.4705f), new(0f, 264.8364f, 0f));
    }
}
