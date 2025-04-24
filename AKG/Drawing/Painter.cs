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
            if (v0.Y > v2.Y) { (v0, v2) = (v2, v0); (n0, n2) = (n2, n0); (worldPos0, worldPos2) = (worldPos2, worldPos0); }
            if (v0.Y > v1.Y) { (v0, v1) = (v1, v0); (n0, n1) = (n1, n0); (worldPos0, worldPos1) = (worldPos1, worldPos0); }
            if (v1.Y > v2.Y) { (v1, v2) = (v2, v1); (n1, n2) = (n2, n1); (worldPos1, worldPos2) = (worldPos2, worldPos1); }

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

        /*#region asdasdsadadas
        public void PaintModelLaba4(Model model)
        {
            // Вычисляем вершины модели в мировом пространстве
            model.CalculateVertices(_buffer.width, _buffer.height);

            // Получаем текстурные координаты
            var textureCoords = model.GetTextureCoords();
            var Normalstmp = model.GetNormals();
            var viewPortVertices = model.GetViewPortVertices();
            var worldVertices = model.GetWorldVertices();
            // Перебираем все грани модели
            foreach (var face in model.GetModelFaces())
            {
                // Вершины треугольника в пространстве экрана
                var v0 = viewPortVertices[face.Indices[0].VertexIndex - 1];
                var v1 = viewPortVertices[face.Indices[1].VertexIndex - 1];
                var v2 = viewPortVertices[face.Indices[2].VertexIndex - 1];

                // Текстурные координаты вершин
                var uv0 = textureCoords[face.Indices[0].TextureIndex - 1];
                var uv1 = textureCoords[face.Indices[1].TextureIndex - 1];
                var uv2 = textureCoords[face.Indices[2].TextureIndex - 1];

                // Мировые координаты вершин
                var wv0 = worldVertices[face.Indices[0].VertexIndex - 1];
                var wv1 = worldVertices[face.Indices[1].VertexIndex - 1];
                var wv2 = worldVertices[face.Indices[2].VertexIndex - 1];

                // Нормали вершин
                var n0 = Normalstmp[face.Indices[0].VertexIndex - 1];
                var n1 = Normalstmp[face.Indices[1].VertexIndex - 1];
                var n2 = Normalstmp[face.Indices[2].VertexIndex - 1];

                // Рисуем треугольник с текстурированием
                DrawTexturedTriangle(
                    new Vector4(v0.X, v0.Y, wv0.Z, v0.W),
                    new Vector4(v1.X, v1.Y, wv1.Z, v1.W),
                    new Vector4(v2.X, v2.Y, wv2.Z, v2.W),
                    uv0, uv1, uv2,
                    n0, n1, n2,
                    model.DiffuseMap, model.NormalMap, model.SpecularMap,
                    model.Ka, model.Kd, model.Ks, model.Shininess,
                    model.lightDir, model.target, model.lightColor);
            }

            // Пишем из буфера на экран
            _buffer.Flush();
        }

        private void DrawTexturedTriangle(
            Vector4 v0, Vector4 v1, Vector4 v2,
            Vector2 uv0, Vector2 uv1, Vector2 uv2,
            Vector3 n0, Vector3 n1, Vector3 n2,
            Bitmap diffuseMap, Bitmap normalMap, Bitmap specularMap,
            Vector3 Ka, Vector3 Kd, Vector3 Ks, float shininess,
            Vector3 lightDir, Vector3 viewDir, Vector3 lightColor)
        {
            // Сортировка вершин по Y (v0.Y <= v1.Y <= v2.Y)
            if (v0.Y > v2.Y) { (v0, v2) = (v2, v0); (uv0, uv2) = (uv2, uv0); (n0, n2) = (n2, n0); }
            if (v0.Y > v1.Y) { (v0, v1) = (v1, v0); (uv0, uv1) = (uv1, uv0); (n0, n1) = (n1, n0); }
            if (v1.Y > v2.Y) { (v1, v2) = (v2, v1); (uv1, uv2) = (uv2, uv1); (n1, n2) = (n2, n1); }

            // Вычисление градиентов для координат X, Z, текстурных координат и нормалей
            var kv1 = (v2 - v0) / (v2.Y - v0.Y);
            var kv2 = (v1 - v0) / (v1.Y - v0.Y);
            var kv3 = (v2 - v1) / (v2.Y - v1.Y);
            var kz1 = (v2.Z - v0.Z) / (v2.Y - v0.Y);
            var kz2 = (v1.Z - v0.Z) / (v1.Y - v0.Y);
            var kz3 = (v2.Z - v1.Z) / (v2.Y - v1.Y);
            var kuv1 = (uv2 - uv0) / (v2.Y - v0.Y);
            var kuv2 = (uv1 - uv0) / (v1.Y - v0.Y);
            var kuv3 = (uv2 - uv1) / (v2.Y - v1.Y);
            var kn1 = (n2 - n0) / (v2.Y - v0.Y);
            var kn2 = (n1 - n0) / (v1.Y - v0.Y);
            var kn3 = (n2 - n1) / (v2.Y - v1.Y);

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

                // Текстурные координаты для крайних точек
                var auv = uv0 + (y - v0.Y) * kuv1;
                var buv = y < v1.Y ? uv0 + (y - v0.Y) * kuv2 : uv1 + (y - v1.Y) * kuv3;

                // Нормали для крайних точек
                var aNormal = n0 + (y - v0.Y) * kn1;
                var bNormal = y < v1.Y ? n0 + (y - v0.Y) * kn2 : n1 + (y - v1.Y) * kn3;

                // Упорядочиваем крайние точки
                if (av.X > bv.X)
                {
                    (av, bv) = (bv, av);
                    (az, bz) = (bz, az);
                    (auv, buv) = (buv, auv);
                    (aNormal, bNormal) = (bNormal, aNormal);
                }

                // Рисуем горизонтальную линию от av.X до bv.X
                var left = Math.Max(0, (int)Math.Ceiling(av.X));
                var right = Math.Min(_buffer.width, (int)Math.Ceiling(bv.X));

                // Цикл по абсциссам
                var kz = (bv.X != av.X) ? (bz - az) / (bv.X - av.X) : 0;
                var kuv = (bv.X != av.X) ? (buv - auv) / (bv.X - av.X) : Vector2.Zero;
                var kNormal = (bv.X != av.X) ? (bNormal - aNormal) / (bv.X - av.X) : Vector3.Zero;

                for (int x = left; x < right; x++)
                {
                    var z = az + (x - av.X) * kz;
                    var uv = auv + (x - av.X) * kuv;
                    var pixelNormal = Vector3.Normalize(aNormal + (x - av.X) * kNormal);

                    // Получаем цвет из текстур
                    //Color diffuseColor = SampleTexture(diffuseMap, uv);
                    Color diffuseColor = (Color.Brown);
                    Color normalColor = SampleTexture(normalMap, uv);
                    Color specularColor = SampleTexture(specularMap, uv);

                    // Преобразуем цвет нормали в вектор
                    Vector3 normalFromMap = new Vector3(
                        normalColor.R / 255f * 2 - 1,
                        normalColor.G / 255f * 2 - 1,
                        normalColor.B / 255f * 2 - 1);

                    // Вычисляем освещение по Фонгу
                    Color pixelColor = CalculatePhongColorWithTextures(
                        new Vector4(x, y, z, 1),
                        pixelNormal, normalFromMap,
                        lightDir, viewDir,
                        Ka, Kd, Ks, shininess, lightColor,
                        diffuseColor, specularColor);

                    // Пишем пиксель в буфер
                    if (_buffer.PutZValue(x, y, z))
                        _buffer[x, y] = pixelColor;
                }
            }
        }

        private Color SampleTexture(Bitmap texture, Vector2 uv)
        {
            int x = (int)(uv.X * texture.Width) % texture.Width;
            int y = (int)(uv.Y * texture.Height) % texture.Height;
            return texture.GetPixel(x, y);
        }

        private Color CalculatePhongColorWithTextures(
            Vector4 position4, Vector3 normal, Vector3 normalFromMap,
            Vector3 lightDir, Vector3 viewDir,
            Vector3 Ka, Vector3 Kd, Vector3 Ks, float shininess, Vector3 lightColor,
            Color diffuseColor, Color specularColor)
        {
            var position = new Vector3(position4.X, position4.Y, position4.Z);
            normal = Vector3.Normalize(normal);
            normalFromMap = Vector3.Normalize(normalFromMap);
            lightDir = Vector3.Normalize(-lightDir);
            viewDir = Vector3.Normalize(viewDir - position);

            // Фоновое освещение
            Vector3 ambient = Ka * lightColor * 1f;

            // Рассеянное освещение
            float diff = Math.Max(Vector3.Dot(normalFromMap, lightDir), 0);
            Vector3 diffuse = new Vector3(diffuseColor.R / 255f, diffuseColor.G / 255f, diffuseColor.B / 255f) * diff * lightColor;

            // Зеркальное освещение
            Vector3 reflectDir = Vector3.Reflect(-lightDir, normalFromMap);
            float spec = (float)Math.Pow(Math.Max(Vector3.Dot(viewDir, reflectDir), 0), shininess);
            Vector3 specular = new Vector3(specularColor.R / 255f, specularColor.G / 255f, specularColor.B / 255f) * spec * lightColor;

            // Суммируем компоненты
            Vector3 finalColor = ambient + diffuse + specular;

            // Преобразуем в Color (ограничиваем значения 0-1)
            finalColor = Vector3.Clamp(finalColor, Vector3.Zero, Vector3.One);
            return Color.FromArgb(
                (int)(finalColor.X * 255),
                (int)(finalColor.Y * 255),
                (int)(finalColor.Z * 255));
        }
        #endregion*/


        /*#region try 3
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
            if (v0.Y < 0 || v1.Y < 0 || v2.Y < 0) return; // Пропускаем треугольники вне экрана
            // Сортировка вершин по Y
            if (v0.Y > v2.Y) (v0, v2) = (v2, v0);
            if (v0.Y > v1.Y) (v0, v1) = (v1, v0);
            if (v1.Y > v2.Y) (v1, v2) = (v2, v1);

            // Градиенты для координат
            var kv1 = (v2 - v0) / (v2.Y - v0.Y);
            var kv2 = (v1 - v0) / (v1.Y - v0.Y);
            var kv3 = (v2 - v1) / (v2.Y - v1.Y);

            // Градиенты для UV
            var kuv1 = (uv2 - uv0) / (v2.Y - v0.Y);
            var kuv2 = (uv1 - uv0) / (v1.Y - v0.Y);
            var kuv3 = (uv2 - uv1) / (v2.Y - v1.Y);

            for (int y = (int)Math.Ceiling(v0.Y); y < Math.Min(_buffer.height, (int)v2.Y); y++)
            {
                var av = v0 + (y - v0.Y) * kv1;
                var bv = (y < v1.Y) ? v0 + (y - v0.Y) * kv2 : v1 + (y - v1.Y) * kv3;

                var auv = (y < v1.Y) ? uv0 + (y - v0.Y) * kuv2 : uv1 + (y - v1.Y) * kuv3;
                var buv = uv0 + (y - v0.Y) * kuv1;

                if (av.X > bv.X) (av, bv) = (bv, av);

                var left = Math.Max(0, (int)Math.Ceiling(av.X));
                var right = Math.Min(_buffer.width, (int)Math.Ceiling(bv.X));

                for (int x = left; x < right; x++)
                {
                    var t = (x - av.X) / (bv.X - av.X);

                    // Перспективная коррекция
                    float one_over_z0 = 1.0f / av.W;
                    float one_over_z1 = 1.0f / bv.W;
                    float one_over_z = one_over_z0 * (1 - t) + one_over_z1 * t;

                    float u_over_z = (auv.X * one_over_z0) * (1 - t) + (buv.X * one_over_z1) * t;
                    float v_over_z = (auv.Y * one_over_z0) * (1 - t) + (buv.Y * one_over_z1) * t;

                    float u = u_over_z / one_over_z;
                    float v = v_over_z / one_over_z;
                    float z = 1.0f / one_over_z;

                    // Получение текстур
                    var diffuseColor = GetTextureColor(model.DiffuseMap, u, v);
                    var specularColor = GetTextureColor(model.SpecularMap, u, v);
                    var normal = GetNormalFromMap(model.NormalMap, u, v);

                    // Расчёт освещения
                    var Ka = new Vector3(diffuseColor.R, diffuseColor.G, diffuseColor.B) / 255f;
                    var Kd = Ka;
                    var Ks = specularColor.R / 255f;

                    var color = CalculatePhongColor(
                        new Vector4(x, y, z, 1),
                        normal,
                        model.lightDir,
                        model.target,
                        Ka,
                        Kd,
                        new Vector3(Ks),
                        model.Shininess,
                        model.lightColor);

                    // Применение цвета
                    if (_buffer.PutZValue(x, y, z))
                        _buffer[x, y] = Color.FromArgb(
                            (int)(color.B * this.B / 255),
                            (int)(color.G * this.G / 255),
                            (int)(color.R * this.R / 255));
                }
            }
        }

        private Color GetTextureColor(Bitmap texture, float u, float v)
        {
            u = u - (float)Math.Floor(u);
            v = v - (float)Math.Floor(v);
            int x = (int)(u * (texture.Width - 1));
            int y = (int)((1 - v) * (texture.Height - 1));
            return texture.GetPixel(x, y);
        }

        private Vector3 GetNormalFromMap(Bitmap normalMap, float u, float v)
        {
            u = u - (float)Math.Floor(u);
            v = v - (float)Math.Floor(v);
            int x = (int)(u * (normalMap.Width - 1));
            int y = (int)((1 - v) * (normalMap.Height - 1));
            var color = normalMap.GetPixel(x, y);
            return new Vector3(
                color.R / 255f * 2 - 1,
                color.G / 255f * 2 - 1,
                color.B / 255f * 2 - 1);
        }
        #endregion*/


        #region 4 laba tmp



        private Color CalculatePhongColor2(Vector4 position4, Vector3 normal, Vector3 lightDir, Vector3 viewDir,
         Vector3 Ka, Vector3 Kd, Vector3 Ks, float shininess, Vector3 lightColor)
        {
            var position = new Vector3(position4.X, position4.Y, position4.Z);
            normal = Vector3.Normalize(normal);
            lightDir = Vector3.Normalize(-lightDir);
            viewDir = Vector3.Normalize(viewDir - position);

            // Фоновое освещение
            Vector3 ambient = Ka * lightColor ;
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
            if (v0.Y > v2.Y) { (v0, v2) = (v2, v0); (uv0, uv2) = (uv2, uv0); }
            if (v0.Y > v1.Y) { (v0, v1) = (v1, v0); (uv0, uv1) = (uv1, uv0); }
            if (v1.Y > v2.Y) { (v1, v2) = (v2, v1); (uv1, uv2) = (uv2, uv1); }

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
                    if (_buffer.PutZValue(x, y, 1/z))
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

         /*#region Четвертая лаба
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
                    //var uv = auv + (buv - auv) * t;


                    var auv_div_z = auv / av.Z;  // uv0 / z0
                    var buv_div_z = buv / bv.Z;  // uv1 / z1
                    var a_inv_z = 1.0f / av.Z;   // 1 / z0
                    var b_inv_z = 1.0f / bv.Z;   // 1 / z1

                    // 2. Линейно интерполируем (uv/z) и (1/z)
                    var uv_div_z_interp = auv_div_z + (buv_div_z - auv_div_z) * t;  // (1-t)*(uv0/z0) + t*(uv1/z1)
                    var inv_z_interp = a_inv_z + (b_inv_z - a_inv_z) * t;           // (1-t)*(1/z0) + t*(1/z1)

                    // 3. Восстанавливаем uv: uv = (uv/z) / (1/z)
                    var uv = uv_div_z_interp / inv_z_interp;

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
                         model.Shininess,
                         model.lightColor);

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
         #endregion*/
    }
}