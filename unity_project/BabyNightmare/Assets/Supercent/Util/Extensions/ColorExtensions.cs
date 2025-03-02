using UnityEngine;

namespace Supercent.Util
{
    public static class ColorExtensions
    {
        public const float Div3 = 1f / 3f;

        public static void SetRed(this UnityEngine.UI.MaskableGraphic image, float red) 
        {
            if (null == image)
                return;

            var color = image.color;
            image.color = new Color(red, color.g, color.b, color.a);
        }

        public static void SetGreen(this UnityEngine.UI.MaskableGraphic image, float green) 
        {
            if (null == image)
                return;

            var color = image.color;
            image.color = new Color(color.r, green, color.b, color.a);
        }

        public static void SetBlue(this UnityEngine.UI.MaskableGraphic image, float blue) 
        {
            if (null == image)
                return;

            var color = image.color;
            image.color = new Color(color.r, color.g, blue, color.a);
        }

        public static void SetAlpha(this UnityEngine.UI.MaskableGraphic image, float alpha) 
        {
            if (null == image)
                return;

            var color = image.color;
            image.color = new Color(color.r, color.g, color.b, alpha);
        }


        public static Color Complementary(this Color color)     => new Color(1f - color.r, 1f - color.g, 1f - color.b, color.a);
        public static Color32 Complementary(this Color32 color) => new Color(byte.MaxValue - color.r, byte.MaxValue - color.g, byte.MaxValue - color.b, color.a);


        public static Color Lerp(this Color c1, Color c2, float t, bool isRGBA)
        {
            if (t < 0f) t = 0f;
            else if (1f < t) t = 1f;

            return new Color(c1.r + (c2.r - (c1.r * t)),
                             c1.g + (c2.g - (c1.g * t)),
                             c1.b + (c2.b - (c1.b * t)),
                             isRGBA ? c1.a + (c2.a - (c1.a * t)) : 1f);
        }
        public static Color LerpUnclamped(this Color c1, Color c2, float t, bool isRGBA)
        {
            return new Color(c1.r + (c2.r - (c1.r * t)),
                             c1.g + (c2.g - (c1.g * t)),
                             c1.b + (c2.b - (c1.b * t)),
                             isRGBA ? c1.a + (c2.a - (c1.a * t)) : 1f);
        }
        public static Color32 Lerp(this Color32 c1, Color32 c2, float t, bool isRGBA)
        {
            if (t < 0f) t = 0f;
            else if (1f < t) t = 1f;

            return new Color32((byte)(c1.r + (c2.r - (c1.r * t))),
                               (byte)(c1.g + (c2.g - (c1.g * t))),
                               (byte)(c1.b + (c2.b - (c1.b * t))),
                               isRGBA ? (byte)(c1.a + (c2.a - (c1.a * t))) : byte.MaxValue);
        }


        public static float GetLightness(this Color c)  => MaxChannel(c, false) + MinChannel(c, false) * 0.5f;
        public static byte GetLightness(this Color32 c) => (byte)((MaxChannel(c, false) + MinChannel(c, false)) >> 1);

        public static float GetAverage(this Color c)    => (c.r + c.g + c.b) * Div3;
        public static byte GetAverage(this Color32 c)   => (byte)((c.r + c.g + c.b) * Div3);

        public static float GetLuminosity(this  Color c)        => (c.r * 0.21f)    + (c.g * 0.72f)     + (c.b * 0.07f);
        public static float GetLuminosity_BT709(this Color c)   => (c.r * 0.2125f)  + (c.g * 0.7154f)   + (c.b * 0.0721f);
        public static float GetLuminosity_RMY(this Color c)     => (c.r * 0.5f)     + (c.g * 0.419f)    + (c.b * 0.081f);
        public static float GetLuminosity_Y(this Color c)       => (c.r * 0.299f)   + (c.g * 0.587f)    + (c.b * 0.114f);

        public static byte GetLuminosity(this Color32 c)        => (byte)((c.r * 0.21f)     + (c.g * 0.72f)     + (c.b * 0.07f));
        public static byte GetLuminosity_BT709(this Color32 c)  => (byte)((c.r * 0.2125f)   + (c.g * 0.7154f)   + (c.b * 0.0721f));
        public static byte GetLuminosity_RMY(this Color32 c)    => (byte)((c.r * 0.5f)      + (c.g * 0.419f)    + (c.b * 0.081f));
        public static byte GetLuminosity_Y(this Color32 c)      => (byte)((c.r * 0.299f)    + (c.g * 0.587f)    + (c.b * 0.114f));


        public static float MinChannel(this Color c, bool isRGBA)
        {
            var result = c.r > c.g ? c.g : c.r;
            result = result > c.b ? c.b : result;
            if (isRGBA)
                result = result > c.a ? c.a : result;
            return result;
        }
        public static byte MinChannel(this Color32 c, bool isRGBA)
        {
            var result = c.r > c.g ? c.g : c.r;
            result = result > c.b ? c.b : result;
            if (isRGBA)
                result = result > c.a ? c.a : result;
            return result;
        }

        public static float MaxChannel(this Color c, bool isRGBA)
        {
            var result = c.r < c.g ? c.g : c.r;
            result = result < c.b ? c.b : result;
            if (isRGBA)
                result = result < c.a ? c.a : result;
            return result;
        }
        public static byte MaxChannel(this Color32 c, bool isRGBA)
        {
            var result = c.r < c.g ? c.g : c.r;
            result = result < c.b ? c.b : result;
            if (isRGBA)
                result = result < c.a ? c.a : result;
            return result;
        }


        public static Color Min(this Color a, Color b, bool isRGBA)
        {
            return new Color(a.r > b.r ? b.r : a.r,
                             a.g > b.g ? b.g : a.g,
                             a.b > b.b ? b.b : a.b,
                             isRGBA ? (a.a > b.a ? b.a : a.a) : 1f);
        }
        public static Color32 Min(this Color32 a, Color32 b, bool isRGBA)
        {
            return new Color(a.r > b.r ? b.r : a.r,
                             a.g > b.g ? b.g : a.g,
                             a.b > b.b ? b.b : a.b,
                             isRGBA ? (a.a > b.a ? b.a : a.a) : byte.MaxValue);
        }

        public static Color Max(this Color a, Color b, bool isRGBA)
        {
            return new Color(a.r < b.r ? b.r : a.r,
                             a.g < b.g ? b.g : a.g,
                             a.b < b.b ? b.b : a.b,
                             isRGBA ? (a.a < b.a ? b.a : a.a) : 1f);
        }
        public static Color32 Max(this Color32 a, Color32 b, bool isRGBA)
        {
            return new Color(a.r < b.r ? b.r : a.r,
                             a.g < b.g ? b.g : a.g,
                             a.b < b.b ? b.b : a.b,
                             isRGBA ? (a.a < b.a ? b.a : a.a) : byte.MaxValue);
        }


        public static Color Clamp(this Color c, Color min, Color max, bool isRGBA)
        {
            return new Color(c.r < min.r ? min.r : c.r > max.r ? max.r : c.r,
                             c.g < min.g ? min.g : c.g > max.g ? max.g : c.g,
                             c.b < min.b ? min.b : c.b > max.b ? max.b : c.b,
                             isRGBA ? (c.a < min.a ? min.a : c.a > max.a ? max.a : c.a) : c.a);
        }
        public static Color32 Clamp(this Color32 c, Color32 min, Color32 max, bool isRGBA)
        {
            return new Color32(c.r < min.r ? min.r : c.r > max.r ? max.r : c.r,
                             c.g < min.g ? min.g : c.g > max.g ? max.g : c.g,
                             c.b < min.b ? min.b : c.b > max.b ? max.b : c.b,
                             isRGBA ? (c.a < min.a ? min.a : c.a > max.a ? max.a : c.a) : c.a);
        }
        public static Color Clamp01(this Color c, bool isRGBA)
        {
            return new Color(c.r < 0f ? 0f : c.r > 1f ? 1f : c.r,
                             c.g < 0f ? 0f : c.g > 1f ? 1f : c.g,
                             c.b < 0f ? 0f : c.b > 1f ? 1f : c.b,
                             isRGBA ? (c.a < 0f ? 0f : c.a > 1f ? 1f : c.a) : c.a);
        }


        #region Color richtext
        public static string WithColor(this string s, string color)
        {
#if UNITY_EDITOR
            return '#' == color[0]
                 ? $"<color={color}>{s}</color>"
                 : $"<color=#{color}>{s}</color>";
#else
            return s;
#endif
        }

        //------------------------------------------------------------------------------
        // pink colors
        //------------------------------------------------------------------------------
        public static string MediumVioletRed(this string s)     => WithColor(s, "#c71585");
        public static string DeepPink(this string s)            => WithColor(s, "#ff1493");
        public static string PaleVioletRed(this string s)       => WithColor(s, "#db7093");
        public static string HotPink(this string s)             => WithColor(s, "#ff69b4");
        public static string LightPink(this string s)           => WithColor(s, "#ffb6c1");
        public static string Pink(this string s)                => WithColor(s, "#ffc0cb");

        //------------------------------------------------------------------------------
        // red colors
        //------------------------------------------------------------------------------
        public static string DarkRed(this string s)             => WithColor(s, "#8b0000");
        public static string Red(this string s)                 => WithColor(s, "#ff0000");
        public static string Firebrick(this string s)           => WithColor(s, "#b22222");
        public static string Crimson(this string s)             => WithColor(s, "#dc143c");
        public static string IndianRed(this string s)           => WithColor(s, "#cd5c5c");
        public static string LightCoral(this string s)          => WithColor(s, "#f08080");
        public static string Salmon(this string s)              => WithColor(s, "#fa8072");
        public static string DarkSalmon(this string s)          => WithColor(s, "#e9967a");
        public static string LightSalmon(this string s)         => WithColor(s, "#ffa07a");

        //------------------------------------------------------------------------------
        // orange colors
        //------------------------------------------------------------------------------
        public static string OrangeRed(this string s)           => WithColor(s, "#ff4500");
        public static string Tomato(this string s)              => WithColor(s, "#ff6347");
        public static string DarkOrange(this string s)          => WithColor(s, "#ff8c00");
        public static string Coral(this string s)               => WithColor(s, "#ff7f50");
        public static string Orange(this string s)              => WithColor(s, "#ffa500");

        //------------------------------------------------------------------------------
        // yellow colors
        //------------------------------------------------------------------------------
        public static string DarkKhaki(this string s)           => WithColor(s, "#bdb76b");
        public static string Gold(this string s)                => WithColor(s, "#ffd700");
        public static string Khaki(this string s)               => WithColor(s, "#f0e68c");
        public static string PeachPuff(this string s)           => WithColor(s, "#ffdab9");
        public static string Yellow(this string s)              => WithColor(s, "#ffff00");
        public static string PaleGoldenrod(this string s)       => WithColor(s, "#eee8aa");
        public static string Moccasian(this string s)           => WithColor(s, "#ffe4b5");
        public static string PapyaWhip(this string s)           => WithColor(s, "#ffefd5");
        public static string LemonChiffon(this string s)        => WithColor(s, "#fffacd");
        public static string LightYellow(this string s)         => WithColor(s, "#ffffe0");

        //------------------------------------------------------------------------------
        // brown colors
        //------------------------------------------------------------------------------
        public static string Maroon(this string s)              => WithColor(s, "#800000");
        public static string Brown(this string s)               => WithColor(s, "#a52a2a");
        public static string SaddleBrown(this string s)         => WithColor(s, "#8b4513");
        public static string Sienna(this string s)              => WithColor(s, "#a0522d");
        public static string Chocolate(this string s)           => WithColor(s, "#d2691e");
        public static string DarkGoldenrod(this string s)       => WithColor(s, "#b8860b");
        public static string Peru(this string s)                => WithColor(s, "#cd853f");
        public static string RosyBrown(this string s)           => WithColor(s, "#bc8f8f");
        public static string Goldenrod(this string s)           => WithColor(s, "#daa520");
        public static string SandyBrown(this string s)          => WithColor(s, "#f4a460");
        public static string Tan(this string s)                 => WithColor(s, "#d2b48c");
        public static string Burlywood(this string s)           => WithColor(s, "#deb887");
        public static string Wheat(this string s)               => WithColor(s, "#f5deb3");
        public static string NavajoWhite(this string s)         => WithColor(s, "#ffdead");
        public static string Bisque(this string s)              => WithColor(s, "#ffe4c4");
        public static string BlanchedAlmond(this string s)      => WithColor(s, "#ffebcd");
        public static string Cornsilk(this string s)            => WithColor(s, "#fff8dc");

        //------------------------------------------------------------------------------
        // green colors
        //------------------------------------------------------------------------------
        public static string DarkGreen(this string s)           => WithColor(s, "#006400");
        public static string Green(this string s)               => WithColor(s, "#008000");
        public static string DarkOliveGreen(this string s)      => WithColor(s, "#556b2f");
        public static string ForestGreen(this string s)         => WithColor(s, "#228b22");
        public static string SeaGreen(this string s)            => WithColor(s, "#2e8b57");
        public static string Olive(this string s)               => WithColor(s, "#808000");
        public static string OliveDrab(this string s)           => WithColor(s, "#6b8e23");
        public static string MediumSeaGreen(this string s)      => WithColor(s, "#3cb371");
        public static string LimeGreen(this string s)           => WithColor(s, "#32cd32");
        public static string Lime(this string s)                => WithColor(s, "#00ff00");
        public static string SpringGreen(this string s)         => WithColor(s, "#00ff7f");
        public static string MediumSpringGreen(this string s)   => WithColor(s, "#00fa9a");
        public static string DarkSeaGreen(this string s)        => WithColor(s, "#8fbc8f");
        public static string MediumAquamarine(this string s)    => WithColor(s, "#66cdaa");
        public static string YellowGreen(this string s)         => WithColor(s, "#9acd32");
        public static string LawnGreen(this string s)           => WithColor(s, "#7cfc00");
        public static string Charteuse(this string s)           => WithColor(s, "#7fff00");
        public static string LightGreen(this string s)          => WithColor(s, "#90ee90");
        public static string GreenYellow(this string s)         => WithColor(s, "#adff2f");
        public static string PaleGreen(this string s)           => WithColor(s, "#98fb98");

        //------------------------------------------------------------------------------
        // cyan colors
        //------------------------------------------------------------------------------
        public static string Teal(this string s)                => WithColor(s, "#008080");
        public static string DarkCyan(this string s)            => WithColor(s, "#008b8b");
        public static string LightSeaGreen(this string s)       => WithColor(s, "#20b2aa");
        public static string CadetBlue(this string s)           => WithColor(s, "#5f9ea0");
        public static string DarkTurquoise(this string s)       => WithColor(s, "#00ced1");
        public static string MediumTurquoise(this string s)     => WithColor(s, "#48d1cc");
        public static string Turquoise(this string s)           => WithColor(s, "#40e0d0");
        public static string Aqua(this string s)                => WithColor(s, "#00ffff");
        public static string Cyan(this string s)                => WithColor(s, "#00ffff");
        public static string Aquamarine(this string s)          => WithColor(s, "#7fffd4");
        public static string PaleTurquoise(this string s)       => WithColor(s, "#afeeee");
        public static string LightCyan(this string s)           => WithColor(s, "#e0ffff");

        //------------------------------------------------------------------------------
        // blue colors
        //------------------------------------------------------------------------------
        public static string Navy(this string s)                => WithColor(s, "#000080");
        public static string DarkBlue(this string s)            => WithColor(s, "#00008b");
        public static string MediumBlue(this string s)          => WithColor(s, "#0000cd");
        public static string Blue(this string s)                => WithColor(s, "#0000ff");
        public static string MidnightBlue(this string s)        => WithColor(s, "#191970");
        public static string RoyalBlue(this string s)           => WithColor(s, "#4169e1");
        public static string SteelBlue(this string s)           => WithColor(s, "#4682b4");
        public static string DodgerBlue(this string s)          => WithColor(s, "#1e90ff");
        public static string DeepSkyBlue(this string s)         => WithColor(s, "#00bfff");
        public static string CornflowerBlue(this string s)      => WithColor(s, "#6495ed");
        public static string SkyBlue(this string s)             => WithColor(s, "#87ceeb");
        public static string LightSkyBlue(this string s)        => WithColor(s, "#87cefa");
        public static string LightSteelBlue(this string s)      => WithColor(s, "#b0c4de");
        public static string LightBlue(this string s)           => WithColor(s, "#add8e6");
        public static string PowderBlue(this string s)          => WithColor(s, "#b0e0e6");

        //------------------------------------------------------------------------------
        // purple, violet, and magenta colors
        //------------------------------------------------------------------------------
        public static string Indigo(this string s)              => WithColor(s, "#4b0082");
        public static string Purple(this string s)              => WithColor(s, "#800080");
        public static string DarkMagenta(this string s)         => WithColor(s, "#8b008b");
        public static string DarkViolet(this string s)          => WithColor(s, "#9400d3");
        public static string DarkSlateBlue(this string s)       => WithColor(s, "#483d8b");
        public static string BlueViolet(this string s)          => WithColor(s, "#8a2be2");
        public static string DarkOrchid(this string s)          => WithColor(s, "#9932cc");
        public static string Fuchsia(this string s)             => WithColor(s, "#ff00ff");
        public static string Magenta(this string s)             => WithColor(s, "#ff00ff");
        public static string SlateBlue(this string s)           => WithColor(s, "#6a5acd");
        public static string MediumSlateBlue(this string s)     => WithColor(s, "#7b68ee");
        public static string MediumOrchid(this string s)        => WithColor(s, "#ba55d3");
        public static string MediumPurple(this string s)        => WithColor(s, "#9370db");
        public static string Orchid(this string s)              => WithColor(s, "#da70d6");
        public static string Violet(this string s)              => WithColor(s, "#ee82ee");
        public static string Plum(this string s)                => WithColor(s, "#dda0dd");
        public static string Thistle(this string s)             => WithColor(s, "#d8bfd8");
        public static string Lavender(this string s)            => WithColor(s, "#e6e6fa");

        //------------------------------------------------------------------------------
        // white colors
        //------------------------------------------------------------------------------
        public static string MistyRose(this string s)           => WithColor(s, "#ffe4e1");
        public static string AntiqueWhite(this string s)        => WithColor(s, "#faebd7");
        public static string Linen(this string s)               => WithColor(s, "#faf0e6");
        public static string Beige(this string s)               => WithColor(s, "#f5f5dc");
        public static string WhiteSmoke(this string s)          => WithColor(s, "#f5f5f5");
        public static string LavenderBlush(this string s)       => WithColor(s, "#fff0f5");
        public static string OldLace(this string s)             => WithColor(s, "#fdf5e6");
        public static string AliceBlue(this string s)           => WithColor(s, "#f0f8ff");
        public static string Seashell(this string s)            => WithColor(s, "#fff5ee");
        public static string GhostWhite(this string s)          => WithColor(s, "#f8f8ff");
        public static string Honeydew(this string s)            => WithColor(s, "#f0fff0");
        public static string FloralWhite(this string s)         => WithColor(s, "#fffaf0");
        public static string Azure(this string s)               => WithColor(s, "#f0ffff");
        public static string MintCream(this string s)           => WithColor(s, "#f5fffa");
        public static string Snow(this string s)                => WithColor(s, "#fffafa");
        public static string Ivory(this string s)               => WithColor(s, "#fffff0");
        public static string White(this string s)               => WithColor(s, "#ffffff");

        //------------------------------------------------------------------------------
        // gray and black colors
        //------------------------------------------------------------------------------
        public static string Black(this string s)               => WithColor(s, "#000000");
        public static string DarkSlateGray(this string s)       => WithColor(s, "#2f4f4f");
        public static string DimGray(this string s)             => WithColor(s, "#696969");
        public static string SlateGray(this string s)           => WithColor(s, "#708090");
        public static string Gray(this string s)                => WithColor(s, "#808080");
        public static string LightSlateGray(this string s)      => WithColor(s, "#778899");
        public static string DarkGray(this string s)            => WithColor(s, "#a9a9a9");
        public static string Silver(this string s)              => WithColor(s, "#c0c0c0");
        public static string LightGray(this string s)           => WithColor(s, "#d3d3d3");
        public static string Gainsboro(this string s)           => WithColor(s, "#dcdcdc");
        #endregion// Color richtext
    }
}