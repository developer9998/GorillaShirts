using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class CaveLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.cave];
        public Vector3 Position => new(-76.981f, -19.429f, -29.6408f);
        public Vector3 EulerAngles => Vector3.up * 92.0393f;
    }
}
