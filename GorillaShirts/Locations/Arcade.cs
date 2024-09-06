using GorillaShirts.Interfaces;
using System;
using UnityEngine;

namespace GorillaShirts.Locations
{
    internal class Arcade : IStandLocation
    {
        public bool IsInZone(GTZone zone) => zone == GTZone.arcade;
        public Tuple<Vector3, Vector3> Location => Tuple.Create<Vector3, Vector3>(new(-33.8662f, 22.0807f, -89.0348f), new(0f, 119.5951f, 0f));
    }
}
