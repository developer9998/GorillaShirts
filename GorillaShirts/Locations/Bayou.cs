using GorillaShirts.Interfaces;
using System;
using UnityEngine;

namespace GorillaShirts.Locations
{
    public class Bayou : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.bayou;
        public Tuple<Vector3, Vector3> Location => Tuple.Create<Vector3, Vector3>(new(-123.1133f, -12.1695f, -91.0308f), new(0f, 34.6322f, 0f));
    }
}
