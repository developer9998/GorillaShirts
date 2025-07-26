using System;

namespace GorillaShirts.Models.Cosmetic
{
    [Flags]
    public enum EShirtFeature
    {
        Audio = 1 << 0,
        Billboard = 1 << 1,
        CustomColours = 1 << 2,
        Invisibility = 1 << 3,
        Light = 1 << 4,
        Particles = 1 << 5,
        Wobble = 1 << 6,
    }
}
