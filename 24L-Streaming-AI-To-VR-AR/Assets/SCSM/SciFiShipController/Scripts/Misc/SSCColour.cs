using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// stores Unity colour data in multiple formats.
    /// RGBA (Color32 and Color), HSV
    /// </summary>
    public struct SSCColour
    {
        #region Public Variables - RGBA colours in 32 bit format
        /// <summary>
        /// Red value 0-255. Set(r,g,b,a) is the same as calling Refresh() after changing the value.
        /// </summary>
        public byte r;
        /// <summary>
        /// Green value 0-255. Set(r,g,b,a) is the same as calling Refresh() after changing the value.
        /// </summary>
        public byte g;
        /// <summary>
        /// Blue value 0-255. Set(r,g,b,a) is the same as calling Refresh() after changing the value.
        /// </summary>
        public byte b;
        /// <summary>
        /// Alpha value 0-255. Set(r,g,b,a) is the same as calling Refresh() after changing the value.
        /// </summary>
        public byte a;
        #endregion

        #region Public Variables - RGBA (float) colour format
        /// <summary>
        /// Red value 0.0-1.0. See also Set(r,g,b,a)
        /// </summary>
        public float rF { get; private set; }
        /// <summary>
        /// Green value 0.0-1.0. See also Set(r,g,b,a)
        /// </summary>
        public float gF { get; private set; }
        /// <summary>
        /// Blue value 0.0-1.0. See also Set(r,g,b,a)
        /// </summary>
        public float bF { get; private set; }
        /// <summary>
        /// Alpha value 0.0-1.0. See also Set(r,g,b,a)
        /// </summary>
        public float aF { get; private set; }
        #endregion

        #region Public Varaibles - HSV colour format
        /// <summary>
        /// Hue
        /// </summary>
        public float h { get; private set; }
        /// <summary>
        /// Satuation
        /// </summary>
        public float s { get; private set; }
        /// <summary>
        /// Lightness value
        /// </summary>
        public float v { get; private set; }
        #endregion

        #region Constructors
        public SSCColour(byte red, byte green, byte blue, byte alpha)
        {
            r = red;
            g = green;
            b = blue;
            a = alpha;

            rF = r / 255f;
            gF = g / 255f;
            bF = b / 255f;
            aF = a / 255f;

            float _h, _s, _v;
            Color.RGBToHSV(new Color(rF, gF, bF, aF), out _h, out _s, out _v);
            h = _h;
            s = _s;
            v = _v;
        }

        public SSCColour(float red, float green, float blue, float alpha)
        {
            rF = red < 0f ? 0f : red > 1f ? 1f : red;
            gF = green < 0f ? 0f : green > 1f ? 1f : green;
            bF = blue < 0f ? 0f : blue > 1f ? 1f : blue;
            aF = alpha < 0f ? 0f : alpha > 1f ? 1f : alpha;

            r = (byte)(rF * 255f);
            g = (byte)(gF * 255f);
            b = (byte)(bF * 255f);
            a = (byte)(aF * 255f);

            float _h, _s, _v;
            Color.RGBToHSV(new Color(rF, gF, bF, aF), out _h, out _s, out _v);
            h = _h;
            s = _s;
            v = _v;
        }

        public SSCColour(Color color)
        {
            r = (byte)(color.r * 255f);
            g = (byte)(color.g * 255f);
            b = (byte)(color.b * 255f);
            a = (byte)(color.a * 255f);

            rF = color.r;
            gF = color.g;
            bF = color.b;
            aF = color.a;

            float _h, _s, _v;
            Color.RGBToHSV(new Color(rF, gF, bF, aF), out _h, out _s, out _v);
            h = _h;
            s = _s;
            v = _v;
        }

        public SSCColour(Color32 color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;

            rF = r / 255f;
            gF = g / 255f;
            bF = b / 255f;
            aF = a / 255f;

            float _h, _s, _v;
            Color.RGBToHSV(new Color(rF, gF, bF, aF), out _h, out _s, out _v);
            h = _h;
            s = _s;
            v = _v;
        }

        #endregion

        #region Private Methods

        private void RefreshHSV()
        {
            float _h, _s, _v;
            Color.RGBToHSV(new Color(rF, gF, bF, aF), out _h, out _s, out _v);
            h = _h;
            s = _s;
            v = _v;
        }

        /// <summary>
        /// Refresh only the Color float data.
        /// Does not include HSV data.
        /// </summary>
        private void RefreshFloatData()
        {
            rF = r / 255f;
            gF = g / 255f;
            bF = b / 255f;
            aF = a / 255f;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Call this after updating the (byte) r, g, b or a values
        /// OR call Set(red, green, blue, alpha).
        /// NOTE: Updating the HSV values will create a temporary Color object.
        /// </summary>
        public void Refresh()
        {
            RefreshFloatData();
            RefreshHSV();
        }

        /// <summary>
        /// Set the byte colour values (0-255) and then refresh all other colour values.
        /// NOTE: Updating the HSV values will create a temporary Color object.
        /// Optionally do not refresh the HSV data
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public void Set(byte red, byte green, byte blue, byte alpha, bool refreshHSV = true)
        {
            r = red;
            g = green;
            b = blue;
            a = alpha;

            if (refreshHSV) { Refresh(); }
            else { RefreshFloatData(); }
        }

        /// <summary>
        /// Set the float colour values (0.0-1.0) and then refresh all other colour values.
        /// NOTE: Updating the HSV values will create a temporary Color object.
        /// Optionally do not refresh the HSV data.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public void Set(float red, float green, float blue, float alpha, bool refreshHSV = true)
        {
            rF = red < 0f ? 0f : red > 1f ? 1f : red;
            gF = green < 0f ? 0f : green > 1f ? 1f : green;
            bF = blue < 0f ? 0f : blue > 1f ? 1f : blue;
            aF = alpha < 0f ? 0f : alpha > 1f ? 1f : alpha;

            // We should always update the core byte rgba data
            r = (byte)(rF * 255f);
            g = (byte)(gF * 255f);
            b = (byte)(bF * 255f);
            a = (byte)(aF * 255f);

            if (refreshHSV) { RefreshHSV(); }
        }

        /// <summary>
        /// Get a new Color struct by applying a brightness factor (0.0-1.0) to the existing colour.
        /// </summary>
        /// <param name="brightness"></param>
        /// <returns></returns>
        public Color GetColorWithBrightness(float brightness)
        {
            // Modify the HSV lightness Value by the brightness level.
            float newV = v * (brightness < 0f ? 0f : brightness > 1f ? 1f : brightness);

            return Color.HSVToRGB(h, s, newV);
        }

        /// <summary>
        /// Get a new Color struct by applying a brightness factor (0.0-1.0) to the existing colour.
        /// Then apply the fade to the alpha channel.
        /// </summary>
        /// <param name="brightness"></param>
        /// <param name="fadeValue"></param>
        /// <returns></returns>
        public Color GetColorWithFadedBrightness(float brightness, float fadeValue)
        {
            // Modify the HSV lightness Value by the brightness level.
            float newV = v * (brightness < 0f ? 0f : brightness > 1f ? 1f : brightness);

            Color _colour = Color.HSVToRGB(h, s, newV);

            _colour.a = fadeValue * aF;

            return _colour;
        }

        #endregion

        #region Implicit Operators
        public static implicit operator SSCColour(Color c)
        {
            return new SSCColour(c);
        }

        public static implicit operator SSCColour(Color32 c)
        {
            return new SSCColour(c);
        }
        #endregion
    }
}
