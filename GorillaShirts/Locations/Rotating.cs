using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Rotating : IStandLocation
    {
        public GTZone Zone => GTZone.rotating;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-68.4334f, -73.54f, -131.5085f), new(0f, 355.1183f, 0f));
    }
}
