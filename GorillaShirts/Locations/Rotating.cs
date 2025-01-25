using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Rotating : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.rotating;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-30.2096f, -10.256f, -341.8227f), new(0f, 180f, 0f));
    }
}
