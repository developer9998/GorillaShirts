using GorillaShirts.Models.Cosmetic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GorillaShirts.Extensions
{
    public static class ShirtExtensions
    {
        public static List<IGorillaShirt> Concat(this IGorillaShirt baseShirt, IList<IGorillaShirt> additionalShirts)
        {
            List<IGorillaShirt> shirts = [baseShirt];

            IEnumerable<EShirtObject> values = Enum.GetValues(typeof(EShirtObject)).Cast<EShirtObject>();

            shirts.AddRange(additionalShirts.Where(shirt => shirt != baseShirt && values.Where(type => shirt.Objects.HasFlag(type)).All(type => !baseShirt.Objects.HasFlag(type))));

            return shirts;
        }
    }
}
