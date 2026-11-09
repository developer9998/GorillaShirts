using UnityEngine;

namespace GorillaShirts.Models.Locations;

internal class Location_GTFC : Location_Base
{
    public override GTZone[] Zones => [GTZone.GTFC];
    public override Vector3  Position => new(236.0192f, 159.4769f, -784.2676f);
    public override Vector3  EulerAngles => Vector3.up * 302.3933f;
}