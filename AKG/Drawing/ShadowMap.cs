using System.Numerics;
using AKG.Realization.Elements;

namespace AKG.Drawing;

public class ShadowMap
{
    public readonly int _width;
    public readonly int _height;
    public readonly float[] _depthBuffer;
    
    public Matrix4x4 LightViewProjectionMatrix { get; private set; }

    public ShadowMap(int width, int height)
    {
        _width = width;
        _height = height;
        _depthBuffer = new float[width * height];
        ClearDepthBuffer();
    }

    public void ClearDepthBuffer()
    {
        Array.Fill(_depthBuffer, float.MinValue);
    }

    public void RenderDepthMap(Model model)
    {
        // 1. Получаем матрицу вида-проекции для источника света
        LightViewProjectionMatrix = model.GetLightViewProjectionMatrix(_width, _height);
        
        // 2. Рассчитываем вершины с точки зрения источника света
        model.CalculateVerticesForShadowMap(_width, _height, LightViewProjectionMatrix);
        
        // 3. Рендерим глубину в буфер
        foreach (var face in model.GetModelFaces())
        {
            var v0 = model.GetViewPortVertices()[face.Indices[0].VertexIndex - 1];
            var v1 = model.GetViewPortVertices()[face.Indices[1].VertexIndex - 1];
            var v2 = model.GetViewPortVertices()[face.Indices[2].VertexIndex - 1];
            
            RenderTriangle(v0, v1, v2);
        }
    }

    private void RenderTriangle(Vector4 v0, Vector4 v1, Vector4 v2)
    {
        // Сортировка вершин по Y
        if (v0.Y > v2.Y) (v0, v2) = (v2, v0);
        if (v0.Y > v1.Y) (v0, v1) = (v1, v0);
        if (v1.Y > v2.Y) (v1, v2) = (v2, v1);

        // Вычисление градиентов
        var deltaY1 = v2.Y - v0.Y;
        var deltaY2 = v1.Y - v0.Y;
        var deltaY3 = v2.Y - v1.Y;

        var kv1 = deltaY1 > 0 ? (v2 - v0) / deltaY1 : Vector4.Zero;
        var kv2 = deltaY2 > 0 ? (v1 - v0) / deltaY2 : Vector4.Zero;
        var kv3 = deltaY3 > 0 ? (v2 - v1) / deltaY3 : Vector4.Zero;

        var kz1 = deltaY1 > 0 ? (v2.Z - v0.Z) / deltaY1 : 0;
        var kz2 = deltaY2 > 0 ? (v1.Z - v0.Z) / deltaY2 : 0;
        var kz3 = deltaY3 > 0 ? (v2.Z - v1.Z) / deltaY3 : 0;

        // Границы растеризации
        var top = Math.Max(0, (int)Math.Ceiling(v0.Y));
        var bottom = Math.Min(_height, (int)Math.Ceiling(v2.Y));

        for (int y = top; y < bottom; y++)
        {
            Vector4 a, b;
            float za, zb;

            if (y < v1.Y)
            {
                float t = y - v0.Y;
                a = v0 + t * kv1;
                b = v0 + t * kv2;
                za = v0.Z + t * kz1;
                zb = v0.Z + t * kz2;
            }
            else
            {
                float t = y - v1.Y;
                a = v1 + t * kv3;
                b = v0 + (y - v0.Y) * kv1;
                za = v1.Z + t * kz3;
                zb = v0.Z + (y - v0.Y) * kz1;
            }

            if (a.X > b.X)
            {
                (a, b) = (b, a);
                (za, zb) = (zb, za);
            }

            int left = Math.Max(0, (int)Math.Ceiling(a.X));
            int right = Math.Min(_width, (int)Math.Ceiling(b.X));

            float zStep = right != left ? (zb - za) / (right - left) : 0;
            
            for (int x = left; x < right; x++)
            {
                float z = za + (x - left) * zStep;
                SetDepth(x, y, z);
            }
        }
    }

    private void SetDepth(int x, int y, float depth)
    {
        int index = y * _width + x;
        if (depth > _depthBuffer[index])
        {
            _depthBuffer[index] = depth;
        }
    }

    public float Sample(float u, float v)
    {
        // Преобразуем UV в координаты текстуры
        int x = (int)(u * (_width - 1));
        int y = (int)((1 - v) * (_height - 1)); // Инвертируем V
        
        // Проверка границ
        x = Math.Clamp(x, 0, _width - 1);
        y = Math.Clamp(y, 0, _height - 1);
        
        return _depthBuffer[y * _width + x];
    }

    public float GetShadowFactor(Vector4 worldPosition)
    {
        // Преобразуем мировые координаты в пространство света
        Vector4 lightSpacePos = Vector4.Transform(worldPosition, LightViewProjectionMatrix);
        lightSpacePos /= lightSpacePos.W;
        
        // Преобразуем в UV координаты [0,1]
        float u = lightSpacePos.X * 0.5f + 0.5f;
        float v = lightSpacePos.Y * 0.5f + 0.5f;
        
        // Проверяем, находится ли точка в пределах shadow map
        if (u < 0 || u > 1 || v < 0 || v > 1)
            return 1.0f;
        
        // Получаем глубину из shadow map
        float shadowDepth = Sample(u, v);
        
        // Сравниваем глубины (добавляем bias для борьбы с артефактами)
        float bias = 0.001f;
        return (lightSpacePos.Z <= shadowDepth + bias) ? 1.0f : 0.2f;
    }
}