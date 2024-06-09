using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Beach : IStandLocation
    {
        public GTZone Zone => GTZone.forest;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(27.21f, 10.2008f, -1.6763f), new(0f, 263.709f, 0f));
    }
}
