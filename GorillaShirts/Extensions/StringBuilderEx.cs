using System.Text;

namespace GorillaShirts.Extensions
{
    /* https://github.com/ToniMacaroni/ComputerInterface/blob/50468f20b4bb7e755d933f8c63627d8cf9394a0e/ComputerInterface/StringBuilderEx.cs */

    public static class StringBuilderEx
    {
        public static StringBuilder AppendClr(this StringBuilder str, string text, string color)
        {
            return str.BeginColor(color).Append(text).EndColor();
        }

        /// <summary>
        /// Writes a string with the specified color
        /// </summary>
        /// <param name="str">the string to print</param>
        /// <param name="color">the hex color (doesn't have to start with '#')</param>
        /// <returns></returns>
        public static StringBuilder BeginColor(this StringBuilder str, string color)
        {
            if (color[0] != '#') color = "#" + color;
            return str.Append($"<color={color}>");
        }

        public static StringBuilder BeginColor(this StringBuilder str, UnityEngine.Color color)
        {
            return str.BeginColor(UnityEngine.ColorUtility.ToHtmlStringRGB(color));
        }

        public static StringBuilder EndColor(this StringBuilder str)
        {
            return str.Append("</color>");
        }

        public static StringBuilder BeginAlign(this StringBuilder str, string align)
        {
            return str.Append($"<align=\"{align}\">");
        }

        public static StringBuilder EndAlign(this StringBuilder str)
        {
            return str.Append("</align>");
        }

        public static StringBuilder BeginCenter(this StringBuilder str)
        {
            return str.BeginAlign("center");
        }

        public static StringBuilder Repeat(this StringBuilder str, string toRepeat, int repeatNum)
        {
            for (int i = 0; i < repeatNum; i++)
            {
                str.Append(toRepeat);
            }

            return str;
        }

        public static StringBuilder AppendLines(this StringBuilder str, int numOfLines)
        {
            str.Repeat("\n", numOfLines);
            return str;
        }

        public static StringBuilder BeginMono(this StringBuilder str, int spacing = 58)
        {
            str.Append("<mspace=58>");
            return str;
        }

        public static StringBuilder EndMono(this StringBuilder str)
        {
            str.Append("</mspace>");
            return str;
        }

        public static StringBuilder AppendMono(this StringBuilder str, string text, int spacing = 58)
        {
            str.BeginMono(spacing).Append(text).EndMono();
            return str;
        }

        public static StringBuilder AppendSize(this StringBuilder str, string text, int size)
        {
            str.Append($"<size={size}%>").Append(text).Append("</size>");
            return str;
        }

        public static StringBuilder BeginVOffset(this StringBuilder str, float offset)
        {
            str.Append($"<voffset={offset}em>");
            return str;
        }

        public static StringBuilder EndVOffset(this StringBuilder str)
        {
            str.Append("</voffset>");
            return str;
        }

        public static StringBuilder MakeBar(this StringBuilder str, char chr, int length, float offset, string color = null)
        {
            str.BeginVOffset(offset);
            if (color != null) str.BeginColor(color);
            str.Repeat(chr.ToString(), length);
            if (color != null) str.EndColor();
            str.EndVOffset();
            return str;
        }

        public static string Clamp(this string str, int length)
        {
            if (str.Length > length)
            {
                var newStr = str.Substring(0, length - 3);
                return newStr + "...";
            }

            return str;
        }
    }
}
