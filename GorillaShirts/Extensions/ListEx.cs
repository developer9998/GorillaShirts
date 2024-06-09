using System;
using System.Collections.Generic;
using System.Linq;

namespace GorillaShirts.Extensions
{
    public static class ListEx
    {
        public static List<T> FromTypeCollection<T>(this List<Type> originalList) => originalList.Select(type => (T)Activator.CreateInstance(type)).ToList();
    }
}
