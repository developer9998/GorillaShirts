using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class MetropolisLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.Metropolis];
        public Vector3 Position => new(-37.0232f, 4.0927f, -138.6099f);
        public Vector3 EulerAngles => Vector3.up * 142.0577f;
    }
}
