namespace GorillaShirts.Models.Cosmetic
{
    public enum EShirtFallback
    {
        None,
        LongsleeveShirt,
        Turtleneck,
        TeeShirt,
        Hoodie,
        Overcoat,
        Croptop,
#if PLUGIN
        SillyCroptop,
        SteadyHoodie
#endif
    }
}
