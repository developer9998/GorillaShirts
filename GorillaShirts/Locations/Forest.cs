using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Forest : IStandLocation
    {
        public GTZone Zone => GTZone.forest;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-67.6651f, 12.07f, -80.438f), new(0f, 171.1801f, 0f));
    }
}
