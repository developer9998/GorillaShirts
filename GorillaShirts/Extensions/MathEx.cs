namespace GorillaShirts.Extensions
{
    public static class MathEx
    {
        public static int Wrap(this int x, int min, int max)
        {
            int range = max - min;
            int result = (x - min) % range;
            if (result < 0)
            {
                result += range;
            }
            return result + min;
        }
    }
}
