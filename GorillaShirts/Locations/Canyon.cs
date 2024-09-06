using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Canyon : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.beach;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-100.246f, 17.9809f, -169.374f), new(0f, 297.5797f, 0f));
    }
}
