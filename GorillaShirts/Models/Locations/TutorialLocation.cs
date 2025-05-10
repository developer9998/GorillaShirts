using UnityEngine;

namespace GorillaShirts.Models.Locations
{
    internal class TutorialLocation : IStandLocation
    {
        public GTZone[] Zones => [GTZone.tutorial];
        public Vector3 Position => new(-98.9992f, 37.6046f, -72.6943f);
        public Vector3 EulerAngles => Vector3.up * 8.7437f;
    }
}
