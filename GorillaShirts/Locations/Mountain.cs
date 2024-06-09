using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Mountain : IStandLocation
    {
        public GTZone Zone => GTZone.forest;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-24.1866f, 18.1936f, -95.9086f), new(0f, 250.1091f, 0f));
    }
}
