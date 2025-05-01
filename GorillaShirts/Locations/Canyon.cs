using System;
using GorillaShirts.Interfaces;

namespace GorillaShirts.Locations
{
    internal class Canyon : IStandLocation
    {
        public float Roof => 16.4754f;
        public bool IsInZone(GTZone zone) => zone == GTZone.canyon;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-100.246f, 17.9809f, -169.374f), new(0f, 297.5797f, 0f));
    }
}
