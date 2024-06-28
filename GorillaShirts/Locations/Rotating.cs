using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Rotating : IStandLocation
    {
        public GTZone Zone => GTZone.rotating;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-72.8298f, -74.271f, -134.549f), new(0f, 301.6311f, 0f));
    }
}
