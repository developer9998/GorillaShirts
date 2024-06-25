using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Forest : IStandLocation
    {
        public GTZone Zone => GTZone.forest;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-65.3888f, 12.05f, -84.3397f), new(0f, 317.6803f, 0f));
    }
}
