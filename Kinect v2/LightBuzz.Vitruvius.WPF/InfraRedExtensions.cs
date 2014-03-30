﻿using Microsoft.Kinect;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LightBuzz.Vitruvius.WPF
{
    /// <summary>
    /// Provides some common functionality for manipulating infrared frames.
    /// </summary>
    public static class InfraredExtensions
    {
        #region Members

        /// <summary>
        /// The bitmap source.
        /// </summary>
        static WriteableBitmap _bitmap = null;

        /// <summary>
        /// The infrared values.
        /// </summary>
        static ushort[] _infraredData = null;

        /// <summary>
        /// The RGB pixel values.
        /// </summary>
        static byte[] _pixels = null;

        #endregion

        #region Public methods

        /// <summary>
        /// Converts an infrared frame to a System.Media.Imaging.BitmapSource.
        /// </summary>
        /// <param name="frame">The specified infrared frame.</param>
        /// <returns>The specified frame in a System.media.Imaging.BitmapSource representation of the infrared frame.</returns>
        public static ImageSource ToBitmap(this InfraredFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;

            if (_bitmap == null)
            {
                _infraredData = new ushort[width * height];
                _pixels = new byte[width * height * Constants.BYTES_PER_PIXEL];
                _bitmap = new WriteableBitmap(width, height, Constants.DPI, Constants.DPI, Constants.FORMAT, null);
            }

            frame.CopyFrameDataToArray(_infraredData);

            // Convert the infrared to RGB.
            int colorIndex = 0;
            for (int infraredIndex = 0; infraredIndex < _infraredData.Length; infraredIndex++)
            {
                // Get the infrared value for this pixel
                ushort ir = _infraredData[infraredIndex];

                // To convert to a byte, we're discarding the most-significant
                // rather than least-significant bits.
                // We're preserving detail, although the intensity will "wrap."
                byte intensity = (byte)(ir >> 6);

                _pixels[colorIndex++] = intensity; // Blue
                _pixels[colorIndex++] = intensity; // Green   
                _pixels[colorIndex++] = intensity; // Red

                // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                // If we were outputting BGRA, we would write alpha here.
                colorIndex++;
            }

            _bitmap.Lock();

            Marshal.Copy(_pixels, 0, _bitmap.BackBuffer, _pixels.Length);
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));

            _bitmap.Unlock();

            return _bitmap;
        }

        #endregion
    }
}
