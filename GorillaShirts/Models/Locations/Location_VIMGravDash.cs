using UnityEngine;

namespace GorillaShirts.Models.Locations;

internal class Location_VIMGravDash : Location_Base
{
    public override GTZone[] Zones    => [GTZone.VIMExperience4]; //VIMGrav-Dash
    public override Vector3  Position => new(177.8102f, 386.4657f, 217.0859f);
    public override Vector3  EulerAngles => Vector3.up * 3.4968f;
}