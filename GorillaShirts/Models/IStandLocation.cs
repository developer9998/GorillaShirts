using UnityEngine;

namespace GorillaShirts.Models
{
    public interface IStandLocation
    {
        public GTZone[] Zones { get; }

        public Vector3 Position { get; }

        public Vector3 EulerAngles { get; }
    }
}
