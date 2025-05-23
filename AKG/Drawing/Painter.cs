using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AKG.Realization;
using AKG.Realization.Elements;
using static System.Math;
using static AKG.Realization.MatrixTransformations;


namespace AKG.Drawing
{
    internal class Painter
    {
        private BuffBitmap _buffer;
        private ShadowMap _shadowMap;

        private readonly int _shadowMapSize = 1024; // Разрешение shadow map

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Painter(Bitmap bitmap)
        {
            _buffer = new BuffBitmap(bitmap);
            _shadowMap = new ShadowMap(_shadowMapSize, _shadowMapSize); // Разрешение shadow map
        }


        #region Первая лаба

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

        #endregion


        #region Вторая лаба

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

        #endregion


        #region Третья лаба

        private void DrawTriangleWithPhongInterpolation(
            Vector4 v0, Vector4 v1, Vector4 v2,
            Vector3 n0, Vector3 n1, Vector3 n2,
            Vector3 worldPos0, Vector3 worldPos1, Vector3 worldPos2,
            Vector3 Ka, Vector3 Kd, Vector3 Ks, float shininess, Vector3 lightDir, Vector3 viewDir, Vector3 lightColor)
        {
            // Сортировка вершин по Y (v0.Y <= v1.Y <= v2.Y)
            if (v0.Y > v2.Y)
            {
                (v0, v2) = (v2, v0);
                (n0, n2) = (n2, n0);
                (worldPos0, worldPos2) = (worldPos2, worldPos0);
            }

            if (v0.Y > v1.Y)
            {
                (v0, v1) = (v1, v0);
                (n0, n1) = (n1, n0);
                (worldPos0, worldPos1) = (worldPos1, worldPos0);
            }

            if (v1.Y > v2.Y)
            {
                (v1, v2) = (v2, v1);
                (n1, n2) = (n2, n1);
                (worldPos1, worldPos2) = (worldPos2, worldPos1);
            }

            // Вычисление градиентов для координат X, Z и нормалей
            var kv1 = (v2 - v0) / (v2.Y - v0.Y);
            var kv2 = (v1 - v0) / (v1.Y - v0.Y);
            var kv3 = (v2 - v1) / (v2.Y - v1.Y);

            var kz1 = (v2.Z - v0.Z) / (v2.Y - v0.Y);
            var kz2 = (v1.Z - v0.Z) / (v1.Y - v0.Y);
            var kz3 = (v2.Z - v1.Z) / (v2.Y - v1.Y);

            // Градиенты для нормалей
            var kn1 = (n2 - n0) / (v2.Y - v0.Y);
            var kn2 = (n1 - n0) / (v1.Y - v0.Y);
            var kn3 = (n2 - n1) / (v2.Y - v1.Y);

            // Градиенты для мировых позиций (для перспективно-корректной интерполяции)
            var kwp1 = (worldPos2 - worldPos0) / (v2.Y - v0.Y);
            var kwp2 = (worldPos1 - worldPos0) / (v1.Y - v0.Y);
            var kwp3 = (worldPos2 - worldPos1) / (v2.Y - v1.Y);

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

                // Нормали для крайних точек
                Vector3 aNormal = n0 + (y - v0.Y) * kn1;
                Vector3 bNormal = y < v1.Y ? n0 + (y - v0.Y) * kn2 : n1 + (y - v1.Y) * kn3;
                
                // Мировые позиции для крайних точек
                Vector3 aWorldPos = worldPos0 + (y - v0.Y) * kwp1;
                Vector3 bWorldPos = y < v1.Y ? worldPos0 + (y - v0.Y) * kwp2 : worldPos1 + (y - v1.Y) * kwp3;

                // Упорядочиваем крайние точки
                if (av.X > bv.X)
                {
                    (av, bv) = (bv, av);
                    (az, bz) = (bz, az);
                    (aNormal, bNormal) = (bNormal, aNormal);
                    (aWorldPos, bWorldPos) = (bWorldPos, aWorldPos);
                }

                // Рисуем горизонтальную линию от av.X до bv.X
                var left = Math.Max(0, (int)Math.Ceiling(av.X));
                var right = Math.Min(_buffer.width, (int)Math.Ceiling(bv.X));

                // Коэффициенты для интерполяции по X
                var kz = (bv.X != av.X) ? (bz - az) / (bv.X - av.X) : 0;
                var kNormal = (bv.X != av.X) ? (bNormal - aNormal) / (bv.X - av.X) : Vector3.Zero;
                var kWorldPos = (bv.X != av.X) ? (bWorldPos - aWorldPos) / (bv.X - av.X) : Vector3.Zero;

                for (int x = left; x < right; x++)
                {
                    var z = az + (x - av.X) * kz;
                    Vector3 pixelNormal = Vector3.Normalize(aNormal + (x - av.X) * kNormal);
                    Vector3 pixelWorldPos = aWorldPos + (x - av.X) * kWorldPos;

                    // Вычисляем цвет по Фонгу для каждого пикселя
                    Color pixelColor = CalculatePhongColor(
                        new Vector4(pixelWorldPos, 1),
                        pixelNormal,
                        lightDir,
                        viewDir,
                        Ka, Kd, Ks, shininess, lightColor);

                    if (_buffer.PutZValue(x, y, z))
                        _buffer[x, y] = pixelColor;
                }
            }
        }

        public void PaintModelLaba3(Model model)
        {
            model.CalculateVertices(_buffer.width, _buffer.height);

            var modelNormals = model.GetModelNormals();
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

                    var n0 = modelNormals[face.Indices[0].VertexIndex - 1];
                    var n1 = modelNormals[face.Indices[1].VertexIndex - 1];
                    var n2 = modelNormals[face.Indices[2].VertexIndex - 1];

                    // Рисуем треугольник с интерполяцией нормалей (Phong shading)
                    DrawTriangleWithPhongInterpolation(
                        new Vector4(v0.X, v0.Y, wv0.Z, v0.W),
                        new Vector4(v1.X, v1.Y, wv1.Z, v1.W),
                        new Vector4(v2.X, v2.Y, wv2.Z, v2.W),
                        n0, n1, n2,
                        new Vector3(wv0.X, wv0.Y, wv0.Z),
                        new Vector3(wv1.X, wv1.Y, wv1.Z),
                        new Vector3(wv2.X, wv2.Y, wv2.Z),
                        model.Ka, model.Kd, model.Ks, model.Shininess,
                        model.lightDir, model.target, model.lightColor);
                }
            }

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
            Vector3 Ka, Vector3 Kd, Vector3 Ks, float shininess, Vector3 lightColor)
        {
            var position = new Vector3(position4.X, position4.Y, position4.Z);
            normal = Vector3.Normalize(normal);
            lightDir = Vector3.Normalize(-lightDir);
            viewDir = Vector3.Normalize(viewDir - position);

            // Фоновое освещение
            Vector3 ambient = Ka * lightColor * 1f;
            // Рассеянное освещение
            float diff = Math.Max(Vector3.Dot(normal, lightDir), 0);
            Vector3 diffuse = Kd * diff * lightColor;
            // Зеркальное освещение
            Vector3 reflectDir = Vector3.Reflect(-lightDir, normal);
            float spec = (float)Math.Pow(Math.Max(Vector3.Dot(viewDir, reflectDir), 0), shininess);
            Vector3 specular = Ks * spec * lightColor;
            // Суммируем компоненты
            Vector3 finalColor = ambient + diffuse + specular;

            // Преобразуем в Color (ограничиваем значения 0-1)
            finalColor = Vector3.Clamp(finalColor, Vector3.Zero, Vector3.One);
            return Color.FromArgb(
                (int)(finalColor.X * this.B),
                (int)(finalColor.Y * this.G),
                (int)(finalColor.Z * this.R));
        }

        #endregion


        #region 4 laba tmp

        private Color CalculatePhongColor2(Vector4 position4, Vector3 normal, Vector3 lightDir, Vector3 viewDir,
            Vector3 Ka, Vector3 Kd, Vector3 Ks, float shininess, Vector3 lightColor)
        {
            var position = new Vector3(position4.X, position4.Y, position4.Z);
            normal = Vector3.Normalize(normal);
            lightDir = Vector3.Normalize(-lightDir);
            viewDir = Vector3.Normalize(viewDir - position);

            // Фоновое освещение
            Vector3 ambient = Ka * lightColor;
            // Рассеянное освещение
            float diff = Math.Max(Vector3.Dot(normal, lightDir), 0);
            Vector3 diffuse = Kd * diff * lightColor;
            // Зеркальное освещение
            Vector3 reflectDir = Vector3.Reflect(-lightDir, normal);
            float spec = (float)Math.Pow(Math.Max(Vector3.Dot(viewDir, reflectDir), 0), shininess);
            Vector3 specular = Ks * spec * lightColor;
            // Суммируем компоненты
            Vector3 finalColor = ambient + diffuse + specular;

            // Преобразуем в Color (ограничиваем значения 0-1)
            finalColor = Vector3.Clamp(finalColor, Vector3.Zero, Vector3.One);
            return Color.FromArgb(
                (int)(finalColor.Z * lightColor.Z * 255),
                (int)(finalColor.Y * lightColor.Y * 255),
                (int)(finalColor.X * lightColor.X * 255));
        }

        public void PaintModelLaba4(Model model)
        {
            model.CalculateVertices(_buffer.width, _buffer.height);

            var modelUVs = model.GetModelTextureCoordinates();

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
                    var wv0 = model.GetWorldVertices()[face.Indices[0].VertexIndex - 1];
                    var wv1 = model.GetWorldVertices()[face.Indices[1].VertexIndex - 1];
                    var wv2 = model.GetWorldVertices()[face.Indices[2].VertexIndex - 1];

                    var uv0 = modelUVs[face.Indices[0].TextureIndex - 1];
                    var uv1 = modelUVs[face.Indices[1].TextureIndex - 1];
                    var uv2 = modelUVs[face.Indices[2].TextureIndex - 1];

                    DrawTexturedTriangle(
                        new Vector4(v0.X, v0.Y, wv0.Z, v0.W),
                        new Vector4(v1.X, v1.Y, wv1.Z, v1.W),
                        new Vector4(v2.X, v2.Y, wv2.Z, v2.W),
                        uv0, uv1, uv2,
                        model);
                }
            }

            _buffer.Flush();
        }

        private void DrawTexturedTriangle(Vector4 v0, Vector4 v1, Vector4 v2,
            Vector3 uv0, Vector3 uv1, Vector3 uv2,
            Model model)
        {
            // Сортировка вершин по Y
            if (v0.Y > v2.Y)
            {
                (v0, v2) = (v2, v0);
                (uv0, uv2) = (uv2, uv0);
            }

            if (v0.Y > v1.Y)
            {
                (v0, v1) = (v1, v0);
                (uv0, uv1) = (uv1, uv0);
            }

            if (v1.Y > v2.Y)
            {
                (v1, v2) = (v2, v1);
                (uv1, uv2) = (uv2, uv1);
            }

            // Вычисляем градиенты для координат X, Z и 1/W (для перспективно-корректной интерполяции)
            float deltaY = v2.Y - v0.Y;
            var kv1 = deltaY != 0 ? (v2 - v0) / deltaY : Vector4.Zero;
            var kuv1 = deltaY != 0 ? (uv2 / v2.W - uv0 / v0.W) / deltaY : Vector3.Zero;
            var kInvW1 = deltaY != 0 ? (1 / v2.W - 1 / v0.W) / deltaY : 0f;

            deltaY = v1.Y - v0.Y;
            var kv2 = deltaY != 0 ? (v1 - v0) / deltaY : Vector4.Zero;
            var kuv2 = deltaY != 0 ? (uv1 / v1.W - uv0 / v0.W) / deltaY : Vector3.Zero;
            var kInvW2 = deltaY != 0 ? (1 / v1.W - 1 / v0.W) / deltaY : 0f;

            deltaY = v2.Y - v1.Y;
            var kv3 = deltaY != 0 ? (v2 - v1) / deltaY : Vector4.Zero;
            var kuv3 = deltaY != 0 ? (uv2 / v2.W - uv1 / v1.W) / deltaY : Vector3.Zero;
            var kInvW3 = deltaY != 0 ? (1 / v2.W - 1 / v1.W) / deltaY : 0f;

            // Границы растеризации
            var top = Math.Max(0, (int)Math.Ceiling(v0.Y));
            var bottom = Math.Min(_buffer.height, (int)Math.Ceiling(v2.Y));

            for (int y = top; y < bottom; y++)
            {
                Vector4 av, bv;
                Vector3 auvPersp, buvPersp;
                float aInvW, bInvW;

                if (y < v1.Y)
                {
                    var t = y - v0.Y;
                    av = v0 + t * kv1;
                    bv = v0 + t * kv2;
                    auvPersp = uv0 / v0.W + t * kuv1;
                    buvPersp = uv0 / v0.W + t * kuv2;
                    aInvW = 1 / v0.W + t * kInvW1;
                    bInvW = 1 / v0.W + t * kInvW2;
                }
                else
                {
                    var t = y - v1.Y;
                    av = v1 + t * kv3;
                    bv = v0 + (y - v0.Y) * kv1;
                    auvPersp = uv1 / v1.W + t * kuv3;
                    buvPersp = uv0 / v0.W + (y - v0.Y) * kuv1;
                    aInvW = 1 / v1.W + t * kInvW3;
                    bInvW = 1 / v0.W + (y - v0.Y) * kInvW1;
                }

                if (av.X > bv.X)
                {
                    (av, bv) = (bv, av);
                    (auvPersp, buvPersp) = (buvPersp, auvPersp);
                    (aInvW, bInvW) = (bInvW, aInvW);
                }

                var left = Math.Max(0, (int)Math.Ceiling(av.X));
                var right = Math.Min(_buffer.width, (int)Math.Ceiling(bv.X));
                var step = bv.X - av.X;
                var k = step != 0 ? 1.0f / step : 0;

                for (int x = left; x < right; x++)
                {
                    var t = (x - av.X) * k;
                    var invW = aInvW * (1 - t) + bInvW * t;
                    var z = 1 / invW; // Корректное значение глубины

                    // Перспективно-корректные UV
                    var uvPersp = auvPersp * (1 - t) + buvPersp * t;
                    var uv = uvPersp * z;

                    // Получаем данные из текстур
                    var texColor = GetTextureColor(model.DiffuseMap, uv.X, uv.Y);
                    var normal = GetNormalFromMap(model.NormalMap, uv.X, uv.Y);
                    var specular = GetSpecularIntensity(model.SpecularMap, uv.X, uv.Y);

                    // Преобразование нормали
                    normal = Vector3.Normalize(normal * 2 - Vector3.One);

                    // Расчет освещения
                    var Ka = new Vector3(texColor.R / 255f, texColor.G / 255f, texColor.B / 255f);
                    var color = CalculatePhongColor2(
                        new Vector4(x, y, z, 1), // Используем корректное значение z
                        normal,
                        model.lightDir,
                        model.target,
                        Ka,
                        Ka,
                        new Vector3(specular),
                        model.Shininess,
                        model.lightColor);

                    // Запись в буфер (используем z для проверки глубины)
                    if (_buffer.PutZValue(x, y, 1 / z))
                        _buffer[x, y] = color;
                }
            }
        }

        private float GetSpecularIntensity(Bitmap specularMap, float u, float v)
        {
            var color = GetTextureColor(specularMap, u, v);
            return (color.R + color.G + color.B) / (3 * 255f);
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

        // Остальные методы без изменений

        #endregion

        #region 5 лаба

        public void PaintModelLaba5(Model model)
        {
            // 1. Рендеринг shadow map (первый проход)
            RenderShadowMapPass(model);

            // 2. Основной рендеринг (второй проход)
            RenderMainPass(model);

            SaveToFile();
            _shadowMap.ClearDepthBuffer();

            _buffer.Flush();
        }

        private void RenderShadowMapPass(Model model)
        {
            _shadowMap.RenderDepthMap(model);
        }

        private void RenderMainPass(Model model)
        {
            // Рассчитываем вершины для основного рендеринга
            model.CalculateVertices(_buffer.width, _buffer.height);
            var viewPortVertices = model.GetViewPortVertices();
            var worldVertices = model.GetWorldVertices();
            var textureCoordinates = model.GetModelTextureCoordinates();

            Matrix4x4 biasMatrix = new Matrix4x4(
                0.5f, 0.0f, 0.0f, 0.0f,
                0.0f, 0.5f, 0.0f, 0.0f,
                0.0f, 0.0f, 0.5f, 0.0f,
                0.5f, 0.5f, 0.5f, 1.0f);
            Matrix4x4 depthBiasMVP = biasMatrix * _shadowMap.LightViewProjectionMatrix;


            // Рендерим все треугольники с учетом теней
            foreach (var face in model.GetModelFaces())
            {
                var v0 = viewPortVertices[face.Indices[0].VertexIndex - 1];
                var v1 = viewPortVertices[face.Indices[1].VertexIndex - 1];
                var v2 = viewPortVertices[face.Indices[2].VertexIndex - 1];
                
                // Мировые координаты вершин
                var wv0 = worldVertices[face.Indices[0].VertexIndex - 1];
                var wv1 = worldVertices[face.Indices[1].VertexIndex - 1];
                var wv2 = worldVertices[face.Indices[2].VertexIndex - 1];
                
                // Текстурные координаты
                var uv0 = textureCoordinates[face.Indices[0].TextureIndex - 1];
                var uv1 = textureCoordinates[face.Indices[1].TextureIndex - 1];
                var uv2 = textureCoordinates[face.Indices[2].TextureIndex - 1];

                // Вычисляем ShadowCoord для каждой вершины
                Vector4 shadowCoord0 = Vector4.Transform(wv0 with { W = 1.0f }, depthBiasMVP);
                Vector4 shadowCoord1 = Vector4.Transform(wv1 with { W = 1.0f }, depthBiasMVP);
                Vector4 shadowCoord2 = Vector4.Transform(wv2 with { W = 1.0f }, depthBiasMVP);

                // Рисуем треугольник с учетом теней
                DrawTexturedTriangleWithShadows(
                    new Vector4(v0.X, v0.Y, wv0.Z, v0.W),
                    new Vector4(v1.X, v1.Y, wv1.Z, v1.W),
                    new Vector4(v2.X, v2.Y, wv2.Z, v2.W),
                    new Vector3(wv0.X, wv0.Y, wv0.Z),
                    new Vector3(wv1.X, wv1.Y, wv1.Z),
                    new Vector3(wv2.X, wv2.Y, wv2.Z),
                    uv0, uv1, uv2,
                    shadowCoord0, shadowCoord1, shadowCoord2,
                    model);
            }
        }

        private void DrawTexturedTriangleWithShadows(
            Vector4 v0, Vector4 v1, Vector4 v2,
            Vector3 wv0, Vector3 wv1, Vector3 wv2,
            Vector3 uv0, Vector3 uv1, Vector3 uv2,
            Vector4 shadowCoord0, Vector4 shadowCoord1, Vector4 shadowCoord2,
            Model model)
        {
            // Сортировка вершин по Y
            if (v0.Y > v2.Y)
            {
                (v0, v2) = (v2, v0);
                (uv0, uv2) = (uv2, uv0);
            }

            if (v0.Y > v1.Y)
            {
                (v0, v1) = (v1, v0);
                (uv0, uv1) = (uv1, uv0);
            }

            if (v1.Y > v2.Y)
            {
                (v1, v2) = (v2, v1);
                (uv1, uv2) = (uv2, uv1);
            }

            // Вычисляем градиенты для координат X, Z и 1/W (для перспективно-корректной интерполяции)
            float deltaY = v2.Y - v0.Y;
            var kv1 = deltaY != 0 ? (v2 - v0) / deltaY : Vector4.Zero;
            var kuv1 = deltaY != 0 ? (uv2 / v2.W - uv0 / v0.W) / deltaY : Vector3.Zero;
            var kInvW1 = deltaY != 0 ? (1 / v2.W - 1 / v0.W) / deltaY : 0f;

            deltaY = v1.Y - v0.Y;
            var kv2 = deltaY != 0 ? (v1 - v0) / deltaY : Vector4.Zero;
            var kuv2 = deltaY != 0 ? (uv1 / v1.W - uv0 / v0.W) / deltaY : Vector3.Zero;
            var kInvW2 = deltaY != 0 ? (1 / v1.W - 1 / v0.W) / deltaY : 0f;

            deltaY = v2.Y - v1.Y;
            var kv3 = deltaY != 0 ? (v2 - v1) / deltaY : Vector4.Zero;
            var kuv3 = deltaY != 0 ? (uv2 / v2.W - uv1 / v1.W) / deltaY : Vector3.Zero;
            var kInvW3 = deltaY != 0 ? (1 / v2.W - 1 / v1.W) / deltaY : 0f;

            // В начале метода (где другие градиенты):
            var kwv1 = deltaY != 0 ? (wv2 - wv0) / deltaY : Vector3.Zero;
            var kwv2 = deltaY != 0 ? (wv1 - wv0) / deltaY : Vector3.Zero;
            var kwv3 = deltaY != 0 ? (wv2 - wv1) / deltaY : Vector3.Zero;

            var kShadow1 = deltaY != 0 ? (shadowCoord2 - shadowCoord0) / deltaY : Vector4.Zero;
            var kShadow2 = deltaY != 0 ? (shadowCoord1 - shadowCoord0) / deltaY : Vector4.Zero;
            var kShadow3 = deltaY != 0 ? (shadowCoord2 - shadowCoord1) / deltaY : Vector4.Zero;

            // Границы растеризации
            var top = Math.Max(0, (int)Math.Ceiling(v0.Y));
            var bottom = Math.Min(_buffer.height, (int)Math.Ceiling(v2.Y));

            for (int y = top; y < bottom; y++)
            {
                Vector4 av, bv;
                Vector3 auvPersp, buvPersp;
                float aInvW, bInvW;
                Vector3 awv, bwv; // Добавьте это вместе с av, bv
                Vector4 aShadowCoord, bShadowCoord; // Добавляем интерполяцию ShadowCoord

                if (y < v1.Y)
                {
                    var t = y - v0.Y;
                    av = v0 + t * kv1;
                    bv = v0 + t * kv2;
                    auvPersp = uv0 / v0.W + t * kuv1;
                    buvPersp = uv0 / v0.W + t * kuv2;
                    aInvW = 1 / v0.W + t * kInvW1;
                    bInvW = 1 / v0.W + t * kInvW2;
                    awv = wv0 + t * kwv1;
                    bwv = wv0 + t * kwv2;
                    aShadowCoord = shadowCoord0 + t * kShadow1;
                    bShadowCoord = shadowCoord0 + t * kShadow2;
                }
                else
                {
                    var t = y - v1.Y;
                    av = v1 + t * kv3;
                    bv = v0 + (y - v0.Y) * kv1;
                    auvPersp = uv1 / v1.W + t * kuv3;
                    buvPersp = uv0 / v0.W + (y - v0.Y) * kuv1;
                    aInvW = 1 / v1.W + t * kInvW3;
                    bInvW = 1 / v0.W + (y - v0.Y) * kInvW1;
                    awv = wv1 + t * kwv3;
                    bwv = wv0 + (y - v0.Y) * kwv1;
                    aShadowCoord = shadowCoord1 + t * kShadow3;
                    bShadowCoord = shadowCoord0 + (y - v0.Y) * kShadow1;
                }
                
                if (av.X > bv.X)
                {
                    (av, bv) = (bv, av);
                    (auvPersp, buvPersp) = (buvPersp, auvPersp);
                    (aInvW, bInvW) = (bInvW, aInvW);
                    (awv, bwv) = (bwv, awv); // Не забудьте поменять местами!
                }

                var left = Math.Max(0, (int)Math.Ceiling(av.X));
                var right = Math.Min(_buffer.width, (int)Math.Ceiling(bv.X));
                var step = bv.X - av.X;
                var k = step != 0 ? 1.0f / step : 0;

                for (int x = left; x < right; x++)
                {
                    var t = (x - av.X) * k;
                    var invW = aInvW * (1 - t) + bInvW * t;
                    var z = 1 / invW; // Корректное значение глубины;

                    // Перспективно-корректные UV
                    var uvPersp = auvPersp * (1 - t) + buvPersp * t;
                    var uv = uvPersp * z;

                    // Получаем данные из текстур
                    var texColor = GetTextureColor(model.DiffuseMap, uv.X, uv.Y);
                    var normal = 
                        GetNormalFromMap(model.NormalMap, uv.X, uv.Y);
                    var specular = GetSpecularIntensity(model.SpecularMap, uv.X, uv.Y);

                    // Преобразование нормали
                    normal = Vector3.Normalize(normal * 2 - Vector3.One);

                    //вот тут возможно что-то не то
                    var worldPos = awv * (1 - t) + bwv * t;                    // Расчет освещения
                    var Ka = new Vector3(texColor.B / 255f, texColor.G / 255f, texColor.R / 255f);

                    var shadowCoord = aShadowCoord * (1 - t) + bShadowCoord * t;

                    // Перспективное деление для ShadowCoord
                    Vector4 projCoords = shadowCoord / shadowCoord.W;
                    float shadowFactor;
                    // Проверяем границы
                    if (projCoords.X < 0 || projCoords.X > 1 ||
                        projCoords.Y < 0 || projCoords.Y > 1)
                    {
                        // Точка вне карты теней - считаем освещенной
                        shadowFactor = 1.0f;
                    }
                    else
                    {
                        // Получаем глубину из shadow map
                        int shadowX = (int)(projCoords.X * (_shadowMap._width - 1));
                        int shadowY = (int)(projCoords.Y * (_shadowMap._height - 1));
                        float closestDepth = _shadowMap.GetDepth(shadowX, shadowY);

                        // Сравниваем глубины с bias
                        const float bias = 0.001f;
                        shadowFactor = (projCoords.Z - bias <= closestDepth) 
                            ? 1.0f 
                            : 0.5f;
                    }

                    var color = CalculatePhongColorWithShadow2(
                        new Vector4(worldPos.X, worldPos.Y, worldPos.Z, 1), // Используем корректное значение z
                        normal, model.lightDir, model.target,
                        model.Ka, model.Kd, model.Ks, model.Shininess,
                        model.lightColor,
                        shadowFactor,
                        model.DiffuseMap,
                        model.NormalMap,
                        model.SpecularMap,
                        new Vector2(uv.X, uv.Y));

                    // Запись в буфер (используем z для проверки глубины)
                    if (_buffer.PutZValue(x, y, 1 / z))
                        _buffer[x, y] = color;
                }
            }
        }

        private Color CalculatePhongColorWithShadow2(
            Vector4 position4,
            Vector3 normal,
            Vector3 lightDir,
            Vector3 viewDir,
            Vector3 Ka,
            Vector3 Kd,
            Vector3 Ks,
            float shininess,
            Vector3 lightColor,
            float shadowFactor, // Добавляем shadow map
            Bitmap diffuseMap = null, // Опциональная диффузная текстура
            Bitmap normalMap = null, // Опциональная normal map
            Bitmap specularMap = null, // Опциональная specular map
            Vector2 uv = default) // Текстурные координаты
        {
            var position = new Vector3(position4.X, position4.Y, position4.Z);

            // Нормализуем векторы
            normal = Vector3.Normalize(normal);
            lightDir = Vector3.Normalize(-lightDir);
            viewDir = Vector3.Normalize(viewDir - position);

            // Цвет из текстуры или белый по умолчанию
            Vector3 baseColor = Vector3.One;
            if (diffuseMap != null)
            {
                Color texColor = GetTextureColor(diffuseMap, uv.X, uv.Y);
                baseColor = new Vector3(texColor.B, texColor.G, texColor.R) / 255.0f;
            }

            // Коррекция нормали с помощью normal map (если есть)
            if (normalMap != null)
            {
                Vector3 texNormal = GetNormalFromMap(normalMap, uv.X, uv.Y);
                texNormal = Vector3.Normalize(texNormal * 2 - Vector3.One); // [0,1] -> [-1,1]
                normal = Vector3.Normalize(normal + texNormal);
            }

            // Интенсивность зеркального отражения из specular map (если есть)
            float specularIntensity = 1.0f;
            if (specularMap != null)
            {
                specularIntensity = GetSpecularIntensity(specularMap, uv.X, uv.Y);
            }

            // Фоновое освещение (не зависит от теней)
            Vector3 ambient = Ka * lightColor * baseColor;

            // Рассеянное освещение (учитывает тени)
            float diff = Math.Max(Vector3.Dot(normal, lightDir), 0);
            Vector3 diffuse = Kd * diff * lightColor * baseColor * shadowFactor;

            // Зеркальное освещение (учитывает тени)
            Vector3 reflectDir = Vector3.Reflect(-lightDir, normal);
            float spec = (float)Math.Pow(Math.Max(Vector3.Dot(viewDir, reflectDir), 0), shininess);
            Vector3 specular = Ks * spec * lightColor * shadowFactor * specularIntensity;

            // Суммируем компоненты
            Vector3 finalColor = ambient + diffuse + specular;
            finalColor = Vector3.Clamp(finalColor, Vector3.Zero, Vector3.One);

            // Преобразуем в Color с учетом настроек R,G,B
            return Color.FromArgb(
                (int)(finalColor.X * B),
                (int)(finalColor.Y * G),
                (int)(finalColor.Z * R));
        }

        private Color CalculatePixelColorWithShadows(
            Vector3 worldPos, Vector3 normal, Vector2 uv, Model model)
        {
            // Получаем shadow factor
            float shadowFactor = _shadowMap.GetShadowFactor(new Vector4(worldPos, 1.0f));

            // Получаем цвет из диффузной текстуры
            Color texColor = GetTextureColor(model.DiffuseMap, uv.X, uv.Y);

            // Получаем нормаль из normal map (если есть)
            if (model.NormalMap != null)
            {
                Vector3 texNormal = GetNormalFromMap(model.NormalMap, uv.X, uv.Y);
                // Преобразуем нормаль из [0,1] в [-1,1]
                texNormal = Vector3.Normalize(texNormal * 2 - Vector3.One);
                normal = Vector3.Normalize(normal + texNormal);
            }

            // Получаем зеркальность из specular map (если есть)
            float specularIntensity = 1.0f;
            if (model.SpecularMap != null)
            {
                specularIntensity = GetSpecularIntensity(model.SpecularMap, uv.X, uv.Y);
            }

            // Векторы для освещения
            Vector3 lightDir = Vector3.Normalize(-model.lightDir);
            Vector3 viewDir = Vector3.Normalize(model.eye - worldPos);

            // Ambient
            Vector3 ambient = model.Ka * new Vector3(texColor.B, texColor.G, texColor.R) / 255.0f;

            // Diffuse
            float diff = Math.Max(Vector3.Dot(normal, lightDir), 0);
            Vector3 diffuse = model.Kd * diff * model.lightColor * shadowFactor;

            // Specular
            Vector3 reflectDir = Vector3.Reflect(-lightDir, normal);
            float spec = (float)Math.Pow(Math.Max(Vector3.Dot(viewDir, reflectDir), model.Shininess), 2);
            Vector3 specular = model.Ks * spec * model.lightColor * shadowFactor * specularIntensity;

            // Итоговый цвет
            Vector3 result = ambient + diffuse + specular;
            result = Vector3.Clamp(result, Vector3.Zero, Vector3.One);

            return Color.FromArgb(
                (int)(result.X * B),
                (int)(result.Y * G),
                (int)(result.Z * R));
        }

        private void SaveToFile()
        {
            int width = _shadowMap._width;
            int height = _shadowMap._height;
            Bitmap bitmap = new Bitmap(width, height);

            // Найдём min и max глубины, чтобы нормализовать
            float min = 0;
            float max = 1;

            foreach (float d in _shadowMap._depthBuffer)
            {
                if (float.IsInfinity(d) || float.IsNaN(d)) continue;
                if (d < min) min = d;
                if (d > max) max = d;
            }

            float range = Math.Max(0.0001f, max - min); // избегаем деления на 0

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    float depth = _shadowMap._depthBuffer[index];

                    Color color = Color.FromArgb((int)(255 * depth), (int)(255 * depth), (int)(255 * depth));
                    bitmap.SetPixel(x, y, color);
                }
            }

            if (File.Exists("shadow.png"))
            {
                File.Delete("shadow.png");
            }

            bitmap.Save("shadow.png", ImageFormat.Png);
        }

        #endregion
    }
}