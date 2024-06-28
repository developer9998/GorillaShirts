using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Metropolis : IStandLocation
    {
        public GTZone Zone => GTZone.Metropolis;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-32.4141f, 4.0927f, -140.1935f), new(0f, 221.5849f, 0f));
    }
}
