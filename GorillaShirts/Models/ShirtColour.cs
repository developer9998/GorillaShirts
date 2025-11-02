using GorillaShirts.Behaviours;
using UnityEngine;

namespace GorillaShirts.Models
{
    public class ShirtColour
    {
        public bool UsePlayerColour { get; set; }
        public Color CustomColour { get; set; }

        public int Data
        {
            get
            {
                if (UsePlayerColour) return -1;

                int r = Mathf.CeilToInt(CustomColour.r * 9f);
                int g = Mathf.CeilToInt(CustomColour.g * 9f);
                int b = Mathf.CeilToInt(CustomColour.b * 9f);
                return r | (g << 8) | (b << 16);
            }
        }

        public void SetData(string shirtId)
        {
            if (DataManager.Instance == null) return;

            int data = Data;
            string key = GetDataKey(shirtId);

            if (data == -1) DataManager.Instance.RemoveItem(key);
            else DataManager.Instance.SetItem(key, data);
        }

        public static ShirtColour FromShirtId(string shirtId)
        {
            if (DataManager.Instance == null) return default;

            ShirtColour colour = new()
            {
                UsePlayerColour = true
            };

            int data = DataManager.Instance.GetItem(GetDataKey(shirtId), -1, false);

            if (data != -1)
            {
                colour.CustomColour = UnpackColour(data);
                colour.UsePlayerColour = false;
            }

            return colour;
        }

        public static Color UnpackColour(int data)
        {
            int r = Mathf.Clamp(data & 255, 0, 9);
            int g = Mathf.Clamp((data >> 8) & 255, 0, 9);
            int b = Mathf.Clamp((data >> 16) & 255, 0, 9);
            return new Color(r / 9f, g / 9f, b / 9f);
        }

        public static int ToDisplaySegment(float value) => Mathf.RoundToInt(Mathf.Lerp(0f, 9f, value));

        public static string GetDataKey(string shirtId) => $"CustomColour_{shirtId}";

        public static explicit operator ShirtColour(int data) => data == -1 ? new ShirtColour()
        {
            UsePlayerColour = true
        } : new ShirtColour()
        {
            UsePlayerColour = false,
            CustomColour = UnpackColour(data)
        };

        public static explicit operator Color?(ShirtColour shirtColour) => shirtColour.UsePlayerColour ? null : shirtColour.CustomColour;

        public override string ToString() => UsePlayerColour ? "Using Player Colour" : CustomColour.ToString();
    }
}
