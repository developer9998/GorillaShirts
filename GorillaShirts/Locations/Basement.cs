using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Basement : IStandLocation
    {
        public GTZone Zone => GTZone.basement;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-36.0703f, 14.4027f, -94.4919f), new(0f, 11.5272f, 0f));
    }
}
