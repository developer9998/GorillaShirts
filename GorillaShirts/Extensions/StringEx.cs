using System;
using UnityEngine;

namespace GorillaShirts.Extensions
{
    public static class StringEx
    {
        public static string NicknameFormat(this string s)
            => new(Array.FindAll(s.ToCharArray(), (c) => char.IsLetterOrDigit(c)));

        public static string LimitString(this string s, int length)
        {
            if (s.Length > Mathf.Abs(length)) s = string.Concat(s.Substring(0, length - 3), "..");
            return s;
        }
    }
}
