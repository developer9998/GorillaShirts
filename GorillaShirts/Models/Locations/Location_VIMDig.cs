using UnityEngine;

namespace GorillaShirts.Models.Locations;

internal class Location_VIMDig : Location_Base
{
    public override GTZone[] Zones => [GTZone.VIMExperience3]; //VIMDig
    public override Vector3  Position => new(194.2696f, 299.096f, 246.8551f);
    public override Vector3  EulerAngles => Vector3.up * 148.146f;
}