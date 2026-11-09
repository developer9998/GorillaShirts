using UnityEngine;

namespace GorillaShirts.Models.Locations;

internal class Location_SilverbackStudios : Location_Base
{
    public override GTZone[] Zones    => [GTZone.SilverbackStudios];
    public override Vector3  Position => new(381.9873f, 164.5481f, -684.8881f);
    public override Vector3  EulerAngles => Vector3.up * 345f;
}