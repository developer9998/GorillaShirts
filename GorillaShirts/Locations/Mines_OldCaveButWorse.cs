using System;
using System.Collections.Generic;
using System.Text;
using GorillaShirts.Interfaces;

namespace GorillaShirts.Locations
{
    internal class Mines_OldCaveButWorse : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.mines;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-45.7829f, -7.323f, -76.7468f), new(0f, 53.7666f, 0f));
    }
}
