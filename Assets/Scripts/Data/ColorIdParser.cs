using System;
using GeckoOut.Core.Board;

namespace GeckoOut.Data
{
    /// <summary>Converts the color strings used in level JSON into ColorId values.</summary>
    public static class ColorIdParser
    {
        public static bool TryParse(string text, out ColorId color)
        {
            color = default;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (!Enum.TryParse(text, true, out color))
            {
                return false;
            }

            // Enum.TryParse also accepts raw numbers like "42" and produces
            // an undefined enum value, so we double-check it is a real color.
            return Enum.IsDefined(typeof(ColorId), color);
        }
    }
}