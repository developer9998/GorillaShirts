namespace GorillaShirts.Models
{
    public class ShirtPair
    {
        public Shirt myShirt;
        public ShirtJSON myDataJSON;

        public ShirtPair(Shirt shirt, ShirtJSON shirtJSON)
        {
            myShirt = shirt;
            myDataJSON = shirtJSON;
        }
    }
}
