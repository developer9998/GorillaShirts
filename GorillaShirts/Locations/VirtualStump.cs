using System;
using GorillaShirts.Interfaces;
using UnityEngine;

namespace GorillaShirts.Locations
{
    internal class VirtualStump : IStandLocation
    {
        public float Roof => -1f;
        public bool IsInZone(GTZone zone) => zone == GTZone.customMaps;
        public Tuple<Vector3, Vector3> Location => Tuple.Create<Vector3, Vector3>(new(1.3714f, -10.4042f, -1.6228f), new(0f, 329.6566f, 0f));
    }
}
