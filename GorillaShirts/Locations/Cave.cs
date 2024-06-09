using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Cave : IStandLocation
    {
        public GTZone Zone => GTZone.forest;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-60.525f, -11.7064f, -41.7656f), new(0f, 267.9607f, 0f));
    }
}
