using System.Collections.Generic;
using System.Linq;
using GorillaShirts.Models;

namespace GorillaShirts.Extensions
{
    public static class ShirtEx
    {
        public static List<IShirtAsset> WithShirts(this IShirtAsset baseShirt, IList<IShirtAsset> additionalShirts)
        {
            List<IShirtAsset> shirts = [baseShirt];

            shirts.AddRange(additionalShirts.Where(shirt => shirt != baseShirt && shirt.ComponentTypes.All(type => !baseShirt.ComponentTypes.Contains(type))));

            return shirts;
        }
    }
}
