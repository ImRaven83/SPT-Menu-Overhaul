using BepInEx.Configuration;
using System;
using System.Globalization;
using UnityEngine;

namespace MoxoPixel.MenuOverhaul.Utils
{
    /// <summary>
    /// BepInEx 6 (IL2CPP) ships no config converter for UnityEngine.Color, unlike BepInEx 5.
    /// Uses the same "RRGGBBAA" hex format BepInEx 5 wrote, so existing config files keep their values.
    /// Implemented without Unity calls because it runs during plugin load.
    /// </summary>
    internal static class ColorTomlConverter
    {
        public static void Register()
        {
            if (TomlTypeConverter.CanConvert(typeof(Color)))
            {
                return;
            }

            TomlTypeConverter.AddConverter(typeof(Color), new TypeConverter
            {
                ConvertToString = (value, type) => ToHex((Color)value),
                ConvertToObject = (text, type) => FromHex(text)
            });
        }

        private static string ToHex(Color color)
        {
            return ToByte(color.r).ToString("X2") + ToByte(color.g).ToString("X2") + ToByte(color.b).ToString("X2") + ToByte(color.a).ToString("X2");
        }

        private static Color FromHex(string text)
        {
            string hex = (text ?? string.Empty).Trim().TrimStart('#');
            if (hex.Length != 6 && hex.Length != 8)
            {
                throw new FormatException($"Invalid color '{text}'. Expected RRGGBB or RRGGBBAA hex.");
            }

            float r = ParseChannel(hex, 0);
            float g = ParseChannel(hex, 2);
            float b = ParseChannel(hex, 4);
            float a = hex.Length == 8 ? ParseChannel(hex, 6) : 1f;
            return new Color(r, g, b, a);
        }

        private static float ParseChannel(string hex, int index)
        {
            return byte.Parse(hex.Substring(index, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255f;
        }

        private static byte ToByte(float channel)
        {
            return (byte)Math.Round(Math.Clamp(channel, 0f, 1f) * 255f);
        }
    }
}
