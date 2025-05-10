using System;
using UnityEngine;

namespace GorillaShirts.Extensions
{
    public static class StringEx
    {
        public static string NicknameFormat(this string s)
        {
            if (s is null)
                return null;

            return new(Array.FindAll(s.ToCharArray(), (c) => char.IsLetterOrDigit(c)));
        }

        public static string LimitString(this string s, int length)
        {
            if (s is null)
                return null;

            return s.Length > Mathf.Abs(length) ? string.Concat(s[..(length - 3)], "..") : s;
        }
    }
}
