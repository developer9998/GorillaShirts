using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class City : IStandLocation
    {
        public GTZone Zone => GTZone.forest;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-53.8759f, 16.7354f, -100.5654f), new(0f, 211.0678f, 0f));
    }
}
