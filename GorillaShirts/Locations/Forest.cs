using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Locations
{
    internal class Forest : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.forest || zone == GTZone.cityWithSkyJungle;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-63.023f, 12.615f, -83.2432f), new(0f, 262f, 0f));
    }
}
