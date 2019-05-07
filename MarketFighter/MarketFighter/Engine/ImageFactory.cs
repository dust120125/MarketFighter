using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarketFighter.Engine
{
    public class ImageFactory
    {

        public static BitmapSource GetColorFilteredBitmap(BitmapSource source, Color color, double opacity)
        {
            if (opacity > 1.0 || opacity < 0)
                throw new ArgumentOutOfRangeException("Opacity range must in '0.0 ~ 1.0'");

            double transparent = 1.0 - opacity;
            byte fR = (byte)(color.R * opacity);
            byte fG = (byte)(color.G * opacity);
            byte fB = (byte)(color.B * opacity);

            WriteableBitmap writeable = new WriteableBitmap(source);
            int stride = writeable.BackBufferStride;
            byte[] pix = new byte[stride * writeable.PixelHeight];
            writeable.CopyPixels(pix, stride, 0);
            Parallel.For(0, writeable.PixelHeight, y =>
            {
                for (int x = 0; x < stride; x += 4)
                {
                    int index = x + y * stride;
                    //bgra
                    byte b = (byte)Math.Min(pix[index] * transparent + fB, 255);
                    byte g = (byte)Math.Min(pix[index + 1] * transparent + fG, 255);
                    byte r = (byte)Math.Min(pix[index + 2] * transparent + fR, 255);
                    //byte a = pix[index + 3];
                    pix[index] = b;
                    pix[index + 1] = g;
                    pix[index + 2] = r;
                    //pix[index + 3] = a;
                }
            });
            writeable.WritePixels(new Int32Rect(0, 0, writeable.PixelWidth, writeable.PixelHeight), pix, stride, 0);
            return writeable;
        }

    }
}
