using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Tutorial : IStandLocation
    {
        public GTZone Zone => GTZone.forest;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-98.9992f, 37.6046f, -72.6943f), new(0f, 8.7437f, 0f));
    }
}
