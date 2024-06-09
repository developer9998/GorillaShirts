using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Clouds : IStandLocation
    {
        public GTZone Zone => GTZone.forest;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-76.7905f, 162.7874f, -100.4427f), new(0f, 342.6743f, 0f));
    }
}
