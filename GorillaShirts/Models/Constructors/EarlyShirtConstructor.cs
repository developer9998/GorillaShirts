using System;

namespace GorillaShirts.Models.Constructors
{
    public class EarlyShirtConstructor : IShirtConstructor // Used for shirts from late 2022 to early-mid 2025
    {
        public int Version { get; } = 1;

        public Shirt GetShirt()
        {
            return null;
        }
    }
}