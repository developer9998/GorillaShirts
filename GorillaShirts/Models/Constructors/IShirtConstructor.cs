namespace GorillaShirts.Models.Constructors
{
    public interface IShirtConstructor
    {
        public int Version { get; }
        public Shirt GetShirt();
    }
}