using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal abstract class Location_Base
    {
        public abstract GTZone[] Zones { get; }
        public abstract Vector3 Position { get; }
        public abstract Vector3 EulerAngles { get; }
    }
}
