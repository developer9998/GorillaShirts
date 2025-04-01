using System;
using UnityEngine;

namespace GorillaShirts.Interfaces
{
    public interface IStandLocation
    {
        public bool IsInZone(GTZone zone);
        public Tuple<Vector3, Vector3> Location { get; }
        public float Roof { get; }
    }
}
