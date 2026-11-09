using UnityEngine;

namespace GorillaShirts.Models.Locations;

internal class Location_FloorIsLava : Location_Base
{
    public override GTZone[] Zones => [GTZone.VIMExperience1]; //FloorIsLava
    public override Vector3  Position => new(210.564f, 74.3629f, 221.9894f);
    public override Vector3  EulerAngles => Vector3.up * 227.8998f;
}