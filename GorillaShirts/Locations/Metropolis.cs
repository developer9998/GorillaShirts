using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Metropolis : IStandLocation
    {
        public float Roof => 4.7927f;
        public bool IsInZone(GTZone zone) => zone == GTZone.Metropolis;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-37.0232f, 4.0927f, -138.6099f), new(0f, 142.0577f, 0f));
    }
}
