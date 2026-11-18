using System;
using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    [Serializable]
    internal abstract class Location_Base
    {
        public abstract GTZone Zone { get; }
        public abstract Vector3 Position { get; }
        public abstract Vector3 EulerAngles { get; }
    }
}
