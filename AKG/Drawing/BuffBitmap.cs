using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Drawing.Image;


namespace AKG.Drawing
{
    internal class BuffBitmap
    {
        private Bitmap _bitmap;
        private byte[] _buffer;
        private double[,] zBuffer;
        private int depth;
        public int width { get; set; }
        public int height { get; set; }

        public BuffBitmap(Bitmap bitmap)
        {
            _bitmap = bitmap;
            width = _bitmap.Width;
            height = _bitmap.Height;
            zBuffer = new double[width, height];
            depth = GetPixelFormatSize(bitmap.PixelFormat) / 8;
            _buffer = new byte[width * height * depth];
        }

        public Color this[int x, int y]
        {
            get
            {
                var offset = (y * width + x) * depth;
                return Color.FromArgb(_buffer[offset], _buffer[offset + 1], _buffer[offset + 2]);
            }
            set
            {
                if (x > 0 && x < width && y > 0 && y < height)
                {
                    var offset = (y * width + x) * depth;
                    if (offset > _buffer.Length) return;
                    _buffer[offset] = value.R;
                    _buffer[offset + 1] = value.G;
                    _buffer[offset + 2] = value.B;
                    if (depth == 4) // Format32bppArgb
                        _buffer[offset + 3] = 255; // Записываем альфа-компонент
                }
            }
        }
        public bool PutZValue(int x, int y, double z)
        {
            double existedZ = zBuffer[x, y];
            if (existedZ < z)
            {
                zBuffer[x, y] = z;
                return true;
            }
            return false;
        }
        private void Reset()
        {
            Array.Fill<byte>(_buffer, 255);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    zBuffer[j, i] = int.MinValue;
        }

        public void Flush() //переписать обратно в bitmap 
        {
            var data = _bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, _bitmap.PixelFormat);
            Marshal.Copy(_buffer, 0, data.Scan0, _buffer.Length);
            _bitmap.UnlockBits(data);
            _bitmap.Save("tmp.png", ImageFormat.Png);
            Reset();
        }

    }
}
