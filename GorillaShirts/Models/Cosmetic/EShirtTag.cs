using System;

namespace GorillaShirts.Models.Cosmetic
{
    [Flags]
    public enum EShirtTag
    {
        Masculine = 1 << 0,
        Feminine = 1 << 1
    }
}
