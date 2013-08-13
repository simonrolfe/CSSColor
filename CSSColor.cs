using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SimonRolfe
{
    static class CSSColor
    {
        /// <summary>
        /// Return a .NET Color from a CSS colour string
        /// </summary>
        /// <param name="CSSColour">The CSS colour to parse. This can be in #rrggbb, #rgb, #rrggbaa, rgb(r, g, b), rgba(r, g, b, a), hsl(h, s, l), hsla(h, s, l, a) format, or a named CSS colour.</param>
        /// <returns>A Color object, if it can be parsed.</returns>
        /// <remarks></remarks>
        public static Color FromCSSString(string CSSColour)
        {
            // empty string check
            if (string.IsNullOrWhiteSpace(CSSColour))
            {
                throw new ArgumentException("Cannot parse an empty colour string.", "CSSColour");
            }
            // determine the format, try standard one first
            Match m1 = Regex.Match(CSSColour, @"^#?([A-F\d]{2})([A-F\d]{2})([A-F\d]{2})([A-F\d]{2})?", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if ((m1.Success) && (m1.Groups.Count == 5))
            {
                if (m1.Groups[4].Value.Length > 0) // includes alpha channel
                    return Color.FromArgb(Byte.Parse(m1.Groups[1].Value, NumberStyles.HexNumber),
                                Byte.Parse(m1.Groups[2].Value, NumberStyles.HexNumber),
                                Byte.Parse(m1.Groups[3].Value, NumberStyles.HexNumber),
                                Byte.Parse(m1.Groups[4].Value, NumberStyles.HexNumber));
                else // colors only
                    return Color.FromArgb(0xFF,
                                Byte.Parse(m1.Groups[1].Value, NumberStyles.HexNumber),
                                Byte.Parse(m1.Groups[2].Value, NumberStyles.HexNumber),
                                Byte.Parse(m1.Groups[3].Value, NumberStyles.HexNumber));
            }
            else
            {
                // try the CSS 3 char format next
                Match m2 = Regex.Match(CSSColour, @"^#?([A-F\d])([A-F\d])([A-F\d])$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if ((m2.Success) && (m2.Groups.Count == 4))
                {
                    byte r = Byte.Parse(m2.Groups[1].Value, NumberStyles.HexNumber);
                    r += (byte)(r << 4);
                    byte g = Byte.Parse(m2.Groups[2].Value, NumberStyles.HexNumber);
                    g += (byte)(g << 4);
                    byte b = Byte.Parse(m2.Groups[3].Value, NumberStyles.HexNumber);
                    b += (byte)(b << 4);
                    return Color.FromArgb(0xFF, r, g, b);
                }
                else
                {
                    if (CSSColour.StartsWith("rgb(") && CSSColour.EndsWith(")"))
                    {
                        string[] rgbTemp = CSSColour.Remove(CSSColour.Length - 1).Remove(0, "rgb(".Length).Split(',');
                        if (rgbTemp.Length == 3)
                        {
                            byte r = ParseRGBComponent(rgbTemp[0]);
                            byte g = ParseRGBComponent(rgbTemp[1]);
                            byte b = ParseRGBComponent(rgbTemp[2]);
                            return Color.FromArgb(0xFF, r, g, b);
                        }
                    }


                    if (CSSColour.StartsWith("rgba(") && CSSColour.EndsWith(")"))
                    {
                        string[] rgbaTemp = CSSColour.Remove(CSSColour.Length - 1).Remove(0, "rgba(".Length).Split(',');

                        if (rgbaTemp.Length == 4)
                        {
                            byte r = ParseRGBComponent(rgbaTemp[0]);
                            byte g = ParseRGBComponent(rgbaTemp[1]);
                            byte b = ParseRGBComponent(rgbaTemp[2]);
                            //todo: alpha should be 0-1 float (clamped) or 0-100%, never 0-255
                            byte a = ParseFloatComponent(rgbaTemp[3]);
                            return Color.FromArgb(a, r, g, b);
                        }
                    }

                    if (CSSColour.StartsWith("hsl(") && CSSColour.EndsWith(")"))
                    {
                        string[] hslTemp = CSSColour.Remove(CSSColour.Length - 1).Remove(0, "hsl(".Length).Split(',');
                        if (hslTemp.Length == 3)
                        {
                            short h = ParseHue(hslTemp[0]);
                            byte s = ParseFloatComponent(hslTemp[1]);
                            byte l = ParseFloatComponent(hslTemp[2]);

                            return HSLA2RGBA(h, s, l);
                        }
                    }

                    if (CSSColour.StartsWith("hsla(") && CSSColour.EndsWith(")"))
                    {
                        string[] hslTemp = CSSColour.Remove(CSSColour.Length - 1).Remove(0, "hsla(".Length).Split(',');
                        if (hslTemp.Length == 4)
                        {
                            short h = ParseHue(hslTemp[0]);
                            byte s = ParseFloatComponent(hslTemp[1]);
                            byte l = ParseFloatComponent(hslTemp[2]);
                            byte a = ParseFloatComponent(hslTemp[3]);

                            return HSLA2RGBA(h, s, l, a);
                        }
                    }

                    switch (CSSColour.ToLower())
                    {
                        case "white":
                        case "silver":
                        case "gray":
                        case "black":
                        case "red":
                        case "maroon":
                        case "yellow":
                        case "olive":
                        case "lime":
                        case "green":
                        case "aqua":
                        case "teal":
                        case "blue":
                        case "navy":
                        case "fuschia":
                        case "purple":
                            return Color.FromName(CSSColour);
                        default:
                            throw new ArgumentException("This is not a valid CSS colour.", "CSSColour");
                    }
                }
            }
        }

        /// <summary>
        /// Returns a Color object from HSL/HSV colours.
        /// </summary>
        /// <param name="h">Hue</param>
        /// <param name="sl">Saturation</param>
        /// <param name="l">Lightness (aka Value)</param>
        /// <remarks>Adapted from http://www.geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm </remarks>
        /// <returns>A Color object representing the colour.</returns>
        public static Color HSLA2RGBA(short Hue, byte Saturation, byte Lightness, byte Alpha = 255)
        {
            double v;
            double r, g, b;

            double h = Hue / 360.0f;
            double sl = Saturation / 255.0f;
            double l = Lightness / 255.0f;

            r = l;   // default to grey
            g = l;
            b = l;

            v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);

            if (v > 0)
            {

                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;

                m = l + l - v;
                sv = (v - m) / v;
                h *= 6.0;
                sextant = (int)h;
                fract = h - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;

                switch (sextant)
                {

                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }

            }
            return Color.FromArgb(Alpha, Convert.ToByte(r * 255.0f), Convert.ToByte(g * 255.0f), Convert.ToByte(b * 255.0f));
        }

        private static byte ParseRGBComponent(string Input)
        {
            string ParseString = Input.Trim();
            if (ParseString.EndsWith("%"))
            {
                return (byte)(ParseClamp(ParseString.Remove(ParseString.Length - 1), 100) * 2.55);
            }
            else
            {
                return (byte)(ParseClamp(ParseString, 255));
            }
        }

        private static byte ParseFloatComponent(string Input)
        {
            string ParseString = Input.Trim();
            if (ParseString.EndsWith("%"))
            {
                return (byte)(ParseClamp(ParseString.Remove(ParseString.Length - 1), 100) * 2.55);
            }
            else
            {
                return (byte)(ParseClamp(ParseString, 1) * 255);
            }
        }

        private static short ParseHue(string Input)
        {
            string ParseString = Input.Trim();
            double ParsedValue;
            if (double.TryParse(Input, out ParsedValue))
            {
                return (short)(((ParsedValue % 360) + 360) % 360);
            }
            else
            {
                throw new ArgumentException("Hue \"" + Input + "\" is not a valid number");
            }
        }

        public static double ParseClamp(string Input, double MaxVal, double MinVal = 0)
        {
            double ParsedValue;
            if (double.TryParse(Input, out ParsedValue))
            {
                if (ParsedValue > MaxVal)
                {
                    return MaxVal;
                }
                if (ParsedValue < MinVal)
                {
                    return MinVal;
                }
                return ParsedValue;
            }
            else
            {
                throw new ArgumentException("Invalid number in input string \"" + Input + "\"");
            }
        }
    }
}
