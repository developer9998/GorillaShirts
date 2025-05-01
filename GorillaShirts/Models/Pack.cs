using System.Collections.Generic;

namespace GorillaShirts.Models
{
    public class Pack<T>
    {
        public string Name;

        public List<T> Items;

        public int Selection;

        public void Navigate(int increment)
        {
            if (Items == null || Items.Count == 0) return;

            int min = 0, max = Items.Count, target = Selection + increment;

            // start of KyleTheScientist/Bark code: https://github.com/KyleTheScientist/Bark/blob/3de171aca033d45f464a5120fb1932c9a0d2a3af/Extensions/MathExtensions.cs#L7

            int range = max - min;
            int result = (target - min) % range;
            if (result < 0)
            {
                result += range;
            }

            // end of KyleTheScientist/Bark code

            Selection = result + min;
        }
    }
}
