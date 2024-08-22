using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Forest : IStandLocation
    {
        public GTZone Zone => GTZone.forest;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-63.023f, 12.6191f, -81.7832f), new(0f, 221.5849f, 0f))
    }
}
