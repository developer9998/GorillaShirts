using System;
using UnityEngine;

namespace GorillaShirts.Interfaces
{
    public interface IStandLocation
    {
        public GTZone Zone { get; }
        public Tuple<Vector3, Vector3> Location { get; }
    }
}
