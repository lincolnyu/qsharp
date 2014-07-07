using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using QSharp.Shader.Graphics.Base.World;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharpTestG
{
    class BitmapImage : RectangularScreen.IImage
    {
        private Bitmap _bitmap;

        public BitmapImage(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public int Height
        {
            get { return _bitmap.Height; }
        }

        public int Width
        {
            get { return _bitmap.Width; }
        }

        public PixelColor8Bit this[int x, int y]
        {
            get
            {
                //if (x <= 0 || x >= Width || y <= 0 || y >= Height) return new PixelColor8Bit(0, 0, 0);
                Color color = _bitmap.GetPixel(x, y);
                return new PixelColor8Bit(color.R, color.G, color.B, color.A);
            }
            set
            {
                //if (x <= 0 || x >= Width || y <= 0 || y >= Height) return;
                _bitmap.SetPixel(x, y, Color.FromArgb(value.Alpha, value.Red, value.Green, value.Blue));
            }
        }
    }
}
