using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Beach : IStandLocation
    {
        public float Roof => 31.721f;
        public bool IsInZone(GTZone zone) => zone == GTZone.beach;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-13.4184f, 28.6591f, -25.4867f), new(0f, 295.2179f, 0f));
    }
}
