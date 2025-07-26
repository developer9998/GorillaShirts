using UnityEngine;

namespace GorillaShirts.Extensions
{
    public static class StringExtensions
    {
        public static string EnforceLength(this string str, int length)
        {
            return str.Length > Mathf.Abs(length) ? string.Concat(str[..(length - 3)], "...") : str;
        }
    }
}
