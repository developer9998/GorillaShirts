using System;
using GorillaShirts.Interfaces;

namespace GorillaShirts.Locations
{
    internal class Mall : IStandLocation
    {
        public float Roof => 19.2925f;
        public bool IsInZone(GTZone zone) => zone == GTZone.mall;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-114.6934f, 16.9925f, -212.6736f), new(0f, 8.6951f, 0f));
    }
}
