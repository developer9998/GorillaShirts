using System;
using GorillaShirts.Interfaces;

namespace GorillaShirts.Locations
{
    internal class MonkeBlocks : IStandLocation
    {
        public float Roof => 17.7881f;
        public bool IsInZone(GTZone zone) => zone == GTZone.monkeBlocks;
        public Tuple<UnityEngine.Vector3, UnityEngine.Vector3> Location => Tuple.Create<UnityEngine.Vector3, UnityEngine.Vector3>(new(-123.7533f, 16.8881f, -219.073f), new(0f, 209.9129f, 0f));
    }
}
