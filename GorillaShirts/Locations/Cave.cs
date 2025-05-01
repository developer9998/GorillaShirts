using System;
using GorillaShirts.Interfaces;

namespace GorillaShirts.Locations
{
    internal class Cave : IStandLocation
    {
        public float Roof => -18.6726f;
        public bool IsInZone(GTZone zone) => zone == GTZone.cave;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-76.981f, -19.429f, -29.6408f), new(0f, 92.0393f, 0f));
    }
}
