using GeckoOut.Core.Board;
using UnityEngine;

namespace GeckoOut.Presentation.Common
{
    /// <summary>Single place where logical colors become visible colors.</summary>
    public static class ColorPalette
    {
        public static Color ToUnityColor(ColorId color)
        {
            switch (color)
            {
                case ColorId.Red: return new Color(0.90f, 0.25f, 0.21f);
                case ColorId.Green: return new Color(0.30f, 0.75f, 0.35f);
                case ColorId.Blue: return new Color(0.25f, 0.50f, 0.90f);
                case ColorId.Yellow: return new Color(0.95f, 0.80f, 0.25f);
                case ColorId.Purple: return new Color(0.60f, 0.35f, 0.80f);
                case ColorId.Orange: return new Color(0.95f, 0.55f, 0.20f);
                default: return Color.white;
            }
        }
    }
}