using System;

namespace GorillaShirts.Models.Cosmetic
{
    [Flags]
    public enum EShirtAnchor
    {
        NameTagAnchor = 1 << 0,
        BadgeAnchor = 1 << 1,
        SlingshotAnchor = 1 << 2
    }
}
