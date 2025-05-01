using System;
using GorillaShirts.Interfaces;

namespace GorillaShirts.Locations
{
    internal class City : IStandLocation
    {
        public float Roof => 16.8854f;
        public bool IsInZone(GTZone zone) => zone == GTZone.city || zone == GTZone.cityNoBuildings;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-51.0506f, 16.7854f, -101.2362f), new(0f, 31.8728f, 0f));
    }
}
