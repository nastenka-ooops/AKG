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
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Painter(Bitmap bitmap)
        {
            _buffer = new BuffBitmap(bitmap);
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


        private void DrawTriangleWithInterpolation(Vector4 v0, Vector4 v1, Vector4 v2, Color c0, Color c1, Color c2)
        {
            // Сортировка вершин по Y (v0.Y <= v1.Y <= v2.Y)
            if (v0.Y > v2.Y)
            {
                (v0, v2) = (v2, v0);
                (c0, c2) = (c2, c0);
            }

            if (v0.Y > v1.Y)
            {
                (v0, v1) = (v1, v0);
                (c0, c1) = (c1, c0);
            }

            if (v1.Y > v2.Y)
            {
                (v1, v2) = (v2, v1);
                (c1, c2) = (c2, c1);
            }

            // Вычисление градиентов для координат X и Z
            var kv1 = (v2 - v0) / (v2.Y - v0.Y);
            var kv2 = (v1 - v0) / (v1.Y - v0.Y);
            var kv3 = (v2 - v1) / (v2.Y - v1.Y);

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

                // Цвета для крайних точек (интерполяция по Y)
                Color aColor, bColor;
                if (y < v1.Y)
                {
                    float t = (y - v0.Y) / (v1.Y - v0.Y);
                    aColor = InterpolateColor(c0, c2, (y - v0.Y) / (v2.Y - v0.Y));
                    bColor = InterpolateColor(c0, c1, t);
                }
                else
                {
                    float t = (y - v1.Y) / (v2.Y - v1.Y);
                    aColor = InterpolateColor(c0, c2, (y - v0.Y) / (v2.Y - v0.Y));
                    bColor = InterpolateColor(c1, c2, t);
                }

                // Упорядочиваем крайние точки
                if (av.X > bv.X)
                {
                    (av, bv) = (bv, av);
                    (az, bz) = (bz, az);
                    (aColor, bColor) = (bColor, aColor);
                }

                // Рисуем горизонтальную линию от av.X до bv.X
                var left = Math.Max(0, (int)Math.Ceiling(av.X));
                var right = Math.Min(_buffer.width, (int)Math.Ceiling(bv.X));

                // Цикл по абсциссам
                var kz = (bv.X != av.X) ? (bz - az) / (bv.X - av.X) : 0;
                var kColor = (bv.X != av.X) ? 1.0f / (bv.X - av.X) : 0;

                for (int x = left; x < right; x++)
                {
                    var z = az + (x - av.X) * kz;
                    float tColor = (x - av.X) * kColor;
                    Color pixelColor = InterpolateColor(aColor, bColor, tColor);

                    if (_buffer.PutZValue(x, y, z))
                        _buffer[x, y] = pixelColor;
                }
            }
        }

        // Вспомогательная функция для интерполяции цвета
        private Color InterpolateColor(Color x, Color y, float t)
        {
            t = Math.Clamp(t, 0, 1);
            int r = (int)(x.R + (y.R - x.R) * t);
            int g = (int)(x.G + (y.G - x.G) * t);
            int b = (int)(x.B + (y.B - x.B) * t);
            return Color.FromArgb(r, g, b);
        }

        private Color CalculatePhongColor(Vector4 position4, Vector3 normal, Vector3 lightDir, Vector3 viewDir,
            Vector3 Ka, Vector3 Kd, Vector3 Ks, float shininess)
        {
            var position = new Vector3(position4.X, position4.Y, position4.Z);
            //var normal = new Vector3(normal4.X, normal4.Y, normal4.Z);
            // Нормализуем векторы
            normal = Vector3.Normalize(normal);
            lightDir = Vector3.Normalize(-lightDir); // Инвертируем, т.к. lightDir обычно направлен ОТ источника
            viewDir = Vector3.Normalize(viewDir - position);

            // Фоновое освещение
            Vector3 ambient = Ka * new Vector3(0.2f, 0.2f, 0.2f); // AmbientColor

            // Рассеянное освещение
            float diff = Math.Max(Vector3.Dot(normal, lightDir), 0);
            Vector3 diffuse = Kd * diff * new Vector3(1.0f, 1.0f, 1.0f); // LightColor

            // Зеркальное освещение
            Vector3 reflectDir = Vector3.Reflect(-lightDir, normal);
            float spec = (float)Math.Pow(Math.Max(Vector3.Dot(viewDir, reflectDir), 0), shininess);
            Vector3 specular = Ks * spec * new Vector3(1.0f, 1.0f, 1.0f); // LightColor

            // Суммируем компоненты
            Vector3 finalColor = ambient + diffuse + specular;

            // Преобразуем в Color (ограничиваем значения 0-1)
            finalColor = Vector3.Clamp(finalColor, Vector3.Zero, Vector3.One);
            return Color.FromArgb(
                (int)(finalColor.X * 255),
                (int)(finalColor.Y * 255),
                (int)(finalColor.Z * 255));
        }

        public void PaintModelLaba3(Model model)
        {
            model.CalculateVertices(_buffer.width, _buffer.height);
            Light lighttmp = new Light
            {
                LightPosition = new Vector3(10f, 10f, 10f),
                LightColor = new Vector3(1f, 1f, 1f),
                AmbientColor = new Vector3(0.2f, 0.2f, 0.2f)
            };
            var modelNormals = model.GetNormals();
            foreach (var face in model.GetModelFaces())
            {
                var v0 = model.GetViewPortVertices()[face.Indices[0].VertexIndex - 1];
                var v1 = model.GetViewPortVertices()[face.Indices[1].VertexIndex - 1];
                var v2 = model.GetViewPortVertices()[face.Indices[2].VertexIndex - 1];

                // Вычисляем нормаль треугольника (для backface culling)
                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var normal = Vector3.Normalize(Vector3.Cross(
                    new Vector3(edge1.X, edge1.Y, edge1.Z),
                    new Vector3(edge2.X, edge2.Y, edge2.Z)));

                if (Vector3.Dot(normal, model.target) > 0)
                {
                    // Мировые координаты вершин
                    var wv0 = model.GetWorldVertices()[face.Indices[0].VertexIndex - 1];
                    var wv1 = model.GetWorldVertices()[face.Indices[1].VertexIndex - 1];
                    var wv2 = model.GetWorldVertices()[face.Indices[2].VertexIndex - 1];


                    // Нормали вершин (должны быть предварительно рассчитаны в модели)

                    //var vertex1 = face.Indices[0].VertexIndex;


                    var n0 = modelNormals[face.Indices[0].VertexIndex - 1];
                    var n1 = modelNormals[face.Indices[1].VertexIndex - 1];
                    var n2 = modelNormals[face.Indices[2].VertexIndex - 1];

                    // Цвета для каждой вершины по Фонгу
                    Color c0 = CalculatePhongColor(wv0, n0, model.lightDir, model.target, model.Ka, model.Kd, model.Ks,
                        model.Shininess);
                    Color c1 = CalculatePhongColor(wv1, n1, model.lightDir, model.target, model.Ka, model.Kd, model.Ks,
                        model.Shininess);
                    Color c2 = CalculatePhongColor(wv2, n2, model.lightDir, model.target, model.Ka, model.Kd, model.Ks,
                        model.Shininess);

                    // красим 

                    Color c00 = Color.FromArgb(255, (int)(c0.B * this.B / 255), (int)(c0.G * this.G / 255),
                        (int)(c0.R * this.R / 255));
                    Color c11 = Color.FromArgb(255, (int)(c1.B * this.B / 255), (int)(c1.G * this.G / 255),
                        (int)(c1.R * this.R / 255));
                    Color c22 = Color.FromArgb(255, (int)(c2.B * this.B / 255), (int)(c2.G * this.G / 255),
                        (int)(c2.R * this.R / 255));


                    // Рисуем треугольник с интерполяцией цветов
                    DrawTriangleWithInterpolation(
                        new Vector4(v0.X, v0.Y, wv0.Z, v0.W),
                        new Vector4(v1.X, v1.Y, wv1.Z, v1.W),
                        new Vector4(v2.X, v2.Y, wv2.Z, v2.W),
                        c00, c11, c22);
                }
            }

            _buffer.Flush();
        }

        public void PaintModelLaba4(Model model)
        {
            model.CalculateVertices(_buffer.width, _buffer.height);
            
            var modelNormals = model.GetNormals();
            var modelUVs = model.GetModelTextureCoordinates(); // Получаем UV-координаты

            foreach (var face in model.GetModelFaces())
            {
                var v0 = model.GetViewPortVertices()[face.Indices[0].VertexIndex - 1];
                var v1 = model.GetViewPortVertices()[face.Indices[1].VertexIndex - 1];
                var v2 = model.GetViewPortVertices()[face.Indices[2].VertexIndex - 1];

                // Backface culling
                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var normal = Vector3.Normalize(Vector3.Cross(
                    new Vector3(edge1.X, edge1.Y, edge1.Z),
                    new Vector3(edge2.X, edge2.Y, edge2.Z)));

                if (Vector3.Dot(normal, model.target) > 0)
                {
                    // Мировые координаты вершин
                    var wv0 = model.GetWorldVertices()[face.Indices[0].VertexIndex - 1];
                    var wv1 = model.GetWorldVertices()[face.Indices[1].VertexIndex - 1];
                    var wv2 = model.GetWorldVertices()[face.Indices[2].VertexIndex - 1];

                    // Нормали вершин
                    var n0 = modelNormals[face.Indices[0].VertexIndex - 1];
                    var n1 = modelNormals[face.Indices[1].VertexIndex - 1];
                    var n2 = modelNormals[face.Indices[2].VertexIndex - 1];

                    // UV-координаты вершин
                    var uv0 = modelUVs[face.Indices[0].TextureIndex - 1];
                    var uv1 = modelUVs[face.Indices[1].TextureIndex - 1];
                    var uv2 = modelUVs[face.Indices[2].TextureIndex - 1];

                    // Рисуем треугольник с учетом текстуры
                    DrawTexturedTriangle(
                        new Vector4(v0.X, v0.Y, wv0.Z, v0.W),
                        new Vector4(v1.X, v1.Y, wv1.Z, v1.W),
                        new Vector4(v2.X, v2.Y, wv2.Z, v2.W),
                        n0, n1, n2,
                        uv0, uv1, uv2,
                        model);
                }
            }

            _buffer.Flush();
        }

        private void DrawTexturedTriangle(Vector4 v0, Vector4 v1, Vector4 v2,
            Vector3 n0, Vector3 n1, Vector3 n2,
            Vector3 uv0, Vector3 uv1, Vector3 uv2,
            Model model)
        {
            // Сортировка вершин по Y (v0.Y <= v1.Y <= v2.Y)
            if (v0.Y > v2.Y)
            {
                (v0, v2) = (v2, v0);
                (n0, n2) = (n2, n0);
                (uv0, uv2) = (uv2, uv0);
            }

            if (v0.Y > v1.Y)
            {
                (v0, v1) = (v1, v0);
                (n0, n1) = (n1, n0);
                (uv0, uv1) = (uv1, uv0);
            }

            if (v1.Y > v2.Y)
            {
                (v1, v2) = (v2, v1);
                (n1, n2) = (n2, n1);
                (uv1, uv2) = (uv2, uv1);
            }

            // Вычисление градиентов для координат X и Z
            var kv1 = (v2 - v0) / (v2.Y - v0.Y);
            var kv2 = (v1 - v0) / (v1.Y - v0.Y);
            var kv3 = (v2 - v1) / (v2.Y - v1.Y);

            // Градиенты для нормалей
            var kn1 = (n2 - n0) / (v2.Y - v0.Y);
            var kn2 = (n1 - n0) / (v1.Y - v0.Y);
            var kn3 = (n2 - n1) / (v2.Y - v1.Y);

            // Градиенты для UV
            var kuv1 = (uv2 - uv0) / (v2.Y - v0.Y);
            var kuv2 = (uv1 - uv0) / (v1.Y - v0.Y);
            var kuv3 = (uv2 - uv1) / (v2.Y - v1.Y);

            // Границы по Y
            var top = Math.Max(0, (int)Math.Ceiling(v0.Y));
            var bottom = Math.Min(_buffer.height, (int)Math.Ceiling(v2.Y));

            // Цикл по строкам
            for (int y = top; y < bottom; y++)
            {
                // Определяем крайние точки
                var av = v0 + (y - v0.Y) * kv1;
                var bv = (y < v1.Y) ? v0 + (y - v0.Y) * kv2 : v1 + (y - v1.Y) * kv3;

                // Нормали для крайних точек
                var an = (y < v1.Y) ? n0 + (y - v0.Y) * kn2 : n1 + (y - v1.Y) * kn3;
                var bn = n0 + (y - v0.Y) * kn1;

                // UV для крайних точек
                var auv = (y < v1.Y) ? uv0 + (y - v0.Y) * kuv2 : uv1 + (y - v1.Y) * kuv3;
                var buv = uv0 + (y - v0.Y) * kuv1;

                // Упорядочиваем крайние точки
                if (av.X > bv.X)
                {
                    (av, bv) = (bv, av);
                    (an, bn) = (bn, an);
                    (auv, buv) = (buv, auv);
                }

                // Рисуем горизонтальную линию от av.X до bv.X
                var left = Math.Max(0, (int)Math.Ceiling(av.X));
                var right = Math.Min(_buffer.width, (int)Math.Ceiling(bv.X));

                // Интерполяция между крайними точками
                var step = bv.X - av.X;
                var k = (step != 0) ? 1.0f / step : 0;

                for (int x = left; x < right; x++)
                {
                    var t = (x - av.X) * k;

                    // Интерполируем Z
                    var z = av.Z + (bv.Z - av.Z) * t;

                    // Интерполируем нормаль
                    //var normal = Vector3.Normalize(an + (bn - an) * t);

                    // Интерполируем UV
                    var uv = auv + (buv - auv) * t;

                    // Получаем цвет из текстуры
                    var texColor = GetTextureColor(model.DiffuseMap, uv.X, uv.Y);

                    // Используем цвет текстуры как kd и ka
                    var Ka = new Vector3(texColor.R / 255f, texColor.G / 255f, texColor.B / 255f);
                    var Kd = Ka; // Обычно диффузная карта используется и для kd и для ka
                    
                    var normalFromMap = GetNormalFromMap(model.NormalMap, uv.X, uv.Y);
            
                    // Преобразуем нормаль из [0,1] в [-1,1] и нормализуем
                    var normal = Vector3.Normalize(normalFromMap * 2 - Vector3.One);

                    // Вычисляем цвет по Фонгу с учетом текстуры
                    var color = CalculatePhongColor(
                        new Vector4(x, y, z, 1),
                        normal,
                        model.lightDir,
                        model.target,
                        Ka,
                        Kd,
                        model.Ks,
                        model.Shininess);

                    // Применяем основной цвет объекта
                    color = Color.FromArgb(
                        (int)(color.B * this.B / 255),
                        (int)(color.G * this.G / 255),
                        (int)(color.R * this.R / 255));

                    if (_buffer.PutZValue(x, y, z))
                        _buffer[x, y] = color;
                }
            }
        }

        private Color GetTextureColor(Bitmap texture, float u, float v)
        {
            // Обеспечиваем повторение текстуры (tiling)
            u = u - (float)Math.Floor(u);
            v = v - (float)Math.Floor(v);

            // Преобразуем UV в координаты текстуры
            int x = (int)(u * (texture.Width - 1));
            int y = (int)((1 - v) * (texture.Height - 1)); // Инвертируем V

            // Получаем цвет текстуры
            return texture.GetPixel(x, y);
        }
        
        private Vector3 GetNormalFromMap(Bitmap normalMap, float u, float v)
        {
            // Обеспечиваем повторение текстуры (tiling)
            u = u - (float)Math.Floor(u);
            v = v - (float)Math.Floor(v);

            // Преобразуем UV в координаты текстуры
            int x = (int)(u * (normalMap.Width - 1));
            int y = (int)((1 - v) * (normalMap.Height - 1)); // Инвертируем V

            // Получаем цвет из карты нормалей
            var color = normalMap.GetPixel(x, y);
    
            // Преобразуем цвет в вектор нормали (значения в [0,1])
            return new Vector3(
                color.R / 255f,
                color.G / 255f,
                color.B / 255f);
        }
    }
}