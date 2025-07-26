using System;

namespace GorillaShirts.Models.Cosmetic
{
    [Flags]
    public enum EShirtObject
    {
        Body = 1 << 0,
        Head = 1 << 1,
        LeftUpper = 1 << 2,
        LeftLower = 1 << 3, // forearm
        LeftHand = 1 << 4,
        RightUpper = 1 << 5,
        RightLower = 1 << 6, // forearm
        RightHand = 1 << 7
    }
}
