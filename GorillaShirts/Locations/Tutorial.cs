using System;
using GorillaShirts.Interfaces;

namespace GorillaShirts.Locations
{
    internal class Tutorial : IStandLocation
    {
        public float Roof => -1f;
        public bool IsInZone(GTZone zone) => zone == GTZone.tutorial;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-98.9992f, 37.6046f, -72.6943f), new(0f, 8.7437f, 0f));
    }
}
