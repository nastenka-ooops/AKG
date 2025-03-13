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
        public int R {  get; set; }
        public int G {  get; set; }
        public int B {  get; set; }

        public Painter(Bitmap bitmap)
        {
            _buffer = new BuffBitmap(bitmap);
            this.R = 0;
            this.G = 0; 
            this.B = 255;
        }

        private void DrawLine(int xStart, int yStart, int xEnd, int yEnd)
        {
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

        public void PaintModelLaba1(Model model)
        {
            model.CalculateVertices(_buffer.width, _buffer.height);

            //переводим
            //рисуем в буфер
            Parallel.ForEach(model.GetModelFaces(), face =>
            {
                var v0 = model.GetViewPortVertices()[face.Indices[0].VertexIndex - 1];
                var v1 = model.GetViewPortVertices()[face.Indices[1].VertexIndex - 1];
                var v2 = model.GetViewPortVertices()[face.Indices[2].VertexIndex - 1];

                DrawLine((int)v0.X, (int)v0.Y, (int)v1.X, (int)v1.Y);
                DrawLine((int)v0.X, (int)v0.Y, (int)v2.X, (int)v2.Y);
                DrawLine((int)v2.X, (int)v2.Y, (int)v1.X, (int)v1.Y);
            });
            //пишем из буфера
            _buffer.Flush();
        }

        private void DrawTriangle(Vector4 v0, Vector4 v1, Vector4 v2, Color color)
        {
            // Сортировка вершин по Y (v0.Y <= v1.Y <= v2.Y)
            if (v0.Y > v2.Y) (v0, v2) = (v2, v0);
            if (v0.Y > v1.Y) (v0, v1) = (v1, v0);
            if (v1.Y > v2.Y) (v1, v2) = (v2, v1);

            // Вычисление градиентов (приращения X на единицу Y)
            var kv1 = (v2 - v0) / (v2.Y - v0.Y);
            var kv2 = (v1 - v0) / (v1.Y - v0.Y);
            var kv3 = (v2 - v1) / (v2.Y - v1.Y);

            // Значения z-буффера для вершин треугольника
            var kz1 = (v2.Z - v0.Z) / (v2.Y - v0.Y);
            var kz2 = (v1.Z - v0.Z) / (v1.Y - v0.Y);
            var kz3 = (v2.Z - v1.Z) / (v2.Y - v1.Y);

            // Границы по Y
            var top = Math.Max(0, (int)Math.Ceiling(v0.Y));
            var bottom = Math.Min(_buffer.height, (int)Math.Ceiling(v2.Y));

            // Цикл по строкам
            for (int y = top; y < bottom; y++)
            {
                // Определяем крайние точки
                var av = v0 + (y - v0.Y) * kv1;
                var bv = (y < v1.Y) ? v0 + (y - v0.Y) * kv2 : v1 + (y - v1.Y) * kv3;

                // Значения z-буффера для крайних точек
                var az = v0.Z + (y - v0.Y) * kz1;
                var bz = y < v1.Y ? v0.Z + (y - v0.Y) * kz2 : v1.Z + (y - v1.Y) * kz3;

                // Упорядочиваем крайние точки
                if (av.X > bv.X) (av, bv) = (bv, av);
                if (av.X > bv.X) (az, bz) = (bz, az);

                // Рисуем горизонтальную линию от av.X до bv.X
                var left = Math.Max(0, (int)Math.Ceiling(av.X));
                var right = Math.Min(_buffer.width, (int)Math.Ceiling(bv.X));

                // Цикл по абсциссам
                var kz = (bz - az) / (bv.X - av.X);
                for (int x = left; x < right; x++)
                {
                    var z = az + (x - av.X) * kz;
                    if (_buffer.PutZValue(x, y, z))
                        _buffer[x, y] = color;
                }
            }
        }

        public void PaintModelLaba2(Model model)
        {
            model.CalculateVertices(_buffer.width, _buffer.height);

            //переводим
            //рисуем в буфер
            foreach (var face in model.GetModelFaces())
            {
                var v0 = model.GetViewPortVertices()[face.Indices[0].VertexIndex - 1];
                var v1 = model.GetViewPortVertices()[face.Indices[1].VertexIndex - 1];
                var v2 = model.GetViewPortVertices()[face.Indices[2].VertexIndex - 1];

                // Вычисляем два ребра треугольника
                var edge1 = v1 - v0;
                var edge2 = v2 - v0;

                // Вычисляем нормаль через векторное произведение
                var normal = Vector3.Cross(
                    new Vector3(edge1.X, edge1.Y, edge1.Z),
                    new Vector3(edge2.X, edge2.Y, edge2.Z));
                normal = Vector3.Normalize(normal);

                // Проверяем, смотрит ли нормаль в сторону наблюдателя
                if (Vector3.Dot(normal, model.target) > 0)
                {
                    // Вычисляем освещенность по Ламберту
                    var vl0 = model.GetWorldVertices()[face.Indices[0].VertexIndex - 1];
                    var vl1 = model.GetWorldVertices()[face.Indices[1].VertexIndex - 1];
                    var vl2 = model.GetWorldVertices()[face.Indices[2].VertexIndex - 1];

                    var lightEdge1 = vl1 - vl0;
                    var lightEdge2 = vl2 - vl0;

                    var lightNormal = Vector3.Normalize(Vector3.Cross(
                        new Vector3(lightEdge1.X, lightEdge1.Y, lightEdge1.Z),
                        new Vector3(lightEdge2.X, lightEdge2.Y, lightEdge2.Z)));
                    float intensity = Vector3.Dot(lightNormal, -model.lightDir);

                    // Определяем цвет треугольника (оттенки синего)
                    int colorValue = (int)(255 * intensity);
                    colorValue = Math.Clamp(colorValue, 0, 255); // Ограничиваем 0-255
                    int r = (int)(this.R * colorValue / 255);
                    int g = (int)(this.G * colorValue / 255);
                    int b = (int)(this.B * colorValue / 255);
                    //Color shadedColor = Color.FromArgb(255, colorValue, colorValue, colorValue); // Голубой с вариациями
                    Color shadedColor = Color.FromArgb(255, b, g, r); // Голубой с вариациями

                    DrawTriangle(
                        new Vector4(v0.X, v0.Y, vl0.Z, v0.W),
                        new Vector4(v1.X, v1.Y, vl1.Z, v1.W),
                        new Vector4(v2.X, v2.Y, vl2.Z, v2.W), 
                        shadedColor);
                }
            }

            //пишем из буфера
            _buffer.Flush();
        }
    }
}