using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Forest : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.forest || zone == GTZone.cityWithSkyJungle;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-64.0157f, 12.51f, -83.8341f), new(0f, 25.7659f, 0f));
    }
}
