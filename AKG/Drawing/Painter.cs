using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AKG.Realization.Elements;
using static System.Math;
using static AKG.Realization.MatrixTransformations;


namespace AKG.Drawing
{
    internal class Painter
    {
        private BuffBitmap _buffer;

        public Painter(Bitmap bitmap)
        {
            _buffer = new BuffBitmap(bitmap);
        }

        private void DrawLine(int xStart, int yStart, int xEnd, int yEnd)
        {
            // Длины проекции на оси абсцисс и ординат
            int dx = xEnd - xStart;
            int dy = yEnd - yStart;

            // Определение сторон сдвига
            int incx = Sign(dx); // -1 для справа налево и +1 для слеванаправо
            int incy = Sign(dy); // -1 для снизу вверх и +1 для сверху вниз

            // Получение абсолютных длин проекций
            dx = Abs(dx);
            dy = Abs(dy);


            int x;
            int y;
            int pdx;
            int pdy;
            int es;
            int el;
            int err;

            // Определение направления прохода в цикле в зависимости от вытянутости
            if (dx > dy)
            {
                // Отрезок более длинный, чем высокий
                pdx = incx;
                pdy = 0;

                es = dy;
                el = dx;
            }
            else
            {
                // Отрезок более высокий, чем длинный
                pdx = 0;
                pdy = incy;

                es = dx;
                el = dy;
            }

            x = xStart;
            y = yStart;
            err = el / 2;


            // Цикл растеризации
            _buffer[x, y] = Color.Black;
            for (int i = 0; i < el; i++)
            {
                err -= es;
                if (err < 0)
                {
                    err += el;

                    // Cдвинуть прямую (сместить вверх или вниз, если цикл проходит по иксам
                    // или сместить влево-вправо, если цикл проходит по y)
                    x += incx;
                    y += incy;
                }
                else
                {
                    // Продолжить тянуть прямую дальше (сдвинуть влево или вправо, если
                    // цикл идёт по иксу; сдвинуть вверх или вниз, если по y)
                    x += pdx;
                    y += pdy;
                }

                _buffer[x, y] = Color.Black;
            }
        }

        private void ScanlineTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Func<Vector3, Color> getColor)
        {
            // Сортировка векторов так, чтобы v2.Y > v1.Y > v0.Y
            if (v0.Y > v2.Y) (v0, v2) = (v2, v0);
            if (v0.Y > v1.Y) (v0, v1) = (v1, v0);
            if (v1.Y > v2.Y) (v1, v2) = (v2, v1);

            // Векторы, коллениарные сторонам, нормы которых равны длине стороны, поделённую на длину проекции на Oy
            var kv1 = (v2 - v0) / (v2.Y - v0.Y);
            var kv2 = (v1 - v0) / (v1.Y - v0.Y);
            var kv3 = (v2 - v1) / (v2.Y - v1.Y);

            // Значения z-буффера для вершин треугольника
            var kz1 = (v2.Z - v0.Z) / (v2.Y - v0.Y);
            var kz2 = (v1.Z - v0.Z) / (v1.Y - v0.Y);
            var kz3 = (v2.Z - v1.Z) / (v2.Y - v1.Y);

            // Границы цикла по ординатам
            var top = Max(0, (int)Ceiling(v0.Y));
            var bottom = Min(_buffer.height, (int)Ceiling(v2.Y));

            // Цикл по ординатам
            for (int y = top; y < bottom; y++)
            {
                // Крайние точки сканирующей линии
                var av = v0 + (y - v0.Y) * kv1;
                var bv = y < v1.Y ? v0 + (y - v0.Y) * kv2 : v1 + (y - v1.Y) * kv3;

                // Значения z-буффера для крайних точек
                var az = v0.Z + (y - v0.Y) * kz1;
                var bz = y < v1.Y ? v0.Z + (y - v0.Y) * kz2 : v1.Z + (y - v1.Y) * kz3;

                // Проход слева направо: сортировка так, чтобы bv.X > av.X
                if (av.X > bv.X)
                {
                    (av, bv) = (bv, av);
                    (az, bz) = (bz, az);
                }

                // Абсциссы крайних точек сканирующей линии
                var left = Max(0, (int)Ceiling(av.X));
                var right = Min(_buffer.width, (int)Ceiling(bv.X));

                // Цикл по абсциссам
                var kz = (bz - az) / (bv.X - av.X);
                for (int x = left; x < right; x++)
                {
                    var z = az + (x - av.X) * kz;
                    if (_buffer.PutZValue(x, y, z))
                        _buffer[x, y] = getColor(new(x, y, z));
                }
            }
        }

        public void PaintModel(Model model)
        {
            model.CalculateVertices(_buffer.width, _buffer.height);

            //переводим
            //рисуем в буфер
            Parallel.ForEach(model.GetModelFaces(), face =>
            {
                var v0 = model.GetModelVertices()[face.Indices[0].VertexIndex - 1];
                var v1 = model.GetModelVertices()[face.Indices[1].VertexIndex - 1];
                var v2 = model.GetModelVertices()[face.Indices[2].VertexIndex - 1];

                DrawLine((int)v0.X, (int)v0.Y, (int)v1.X, (int)v1.Y);
                DrawLine((int)v0.X, (int)v0.Y, (int)v2.X, (int)v2.Y);
                DrawLine((int)v2.X, (int)v2.Y, (int)v1.X, (int)v1.Y);
            });
            //пишем из буфера
            _buffer.Flush();
        }
    }
}