using UnityEngine;

namespace GorillaShirts.Models.Locations;

internal class Location_SpaceMap : Location_Base
{
    public override GTZone[] Zones => [GTZone.spaceMap];
    public override Vector3  Position => new(-548.0914f, 12.4272f, -0.008f);
    public override Vector3  EulerAngles => Vector3.up * 38.7456f;
}