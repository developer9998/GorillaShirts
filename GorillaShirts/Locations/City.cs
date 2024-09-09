using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class City : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.city || zone == GTZone.cityNoBuildings;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-50.676f, 16.7854f, -99.5654f), new(0f, 211.0678f, 0f));
    }
}
