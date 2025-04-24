using AKG.Realization.Elements;
using System;
using System.Collections.Generic;
using System.Numerics;
using static System.Numerics.Matrix4x4;

namespace AKG.Realization
{
    public static class MatrixTransformations
    {
        //создаем матрицу для приведения к мировым координатам
        //Shift - сдвиг по осям Rotate - поворот по осям Scale - масштаб
        public static Vector4 TransformToWordMatrix(Vector4 vertex, float ShiftX, float ShiftY, float ShiftZ,
            float RotateX, float RotateY, float RotateZ, float scale)
        {
            var worldMatr = CreateTranslation(ShiftX, ShiftY, ShiftZ) // матрица для сдвига
                            * CreateRotationX(RotateX) * CreateRotationY(RotateY) *
                            CreateRotationZ(RotateZ) // матрицы для поворотов
                            * CreateScale(scale); // матрица для масштаба
            return Transform(vertex, worldMatr);
        }

        // Создает матрицу, которая преобразует координаты из мирового пространства в пространство наблюдателя.
        // eye - Позиция камеры (где она находится).
        // target - Позиция цели (куда смотрит камера).
        // up - Вектор "вверх" (обычно (0,1,0))
        public static Vector4 TransformToViewMatrix(Vector4 vertex, Vector3 eye, Vector3 target, Vector3 up)
        {
            return Transform(vertex, Matrix4x4.CreateLookAt(eye, target, up));
        }

        public static Vector4 TransformToPerspectiveProjectionMatrix(Vector4 vertex, float fov, float aspect, 
            float zNear, float zFar)
        {
            var transformVertex = Transform(vertex, Matrix4x4.CreatePerspectiveFieldOfView(
                fov, aspect, zFar, zNear));
            return transformVertex;
            return new Vector4(transformVertex.X / transformVertex.W, transformVertex.Y / transformVertex.W,
                transformVertex.Z / transformVertex.W, transformVertex.W / transformVertex.W);
        }

        // Создает матрицу преобразования в пространство окна просмотра (viewport).
        //width - Ширина окна просмотра
        //height - Высота окна просмотра
        //xmin - Минимальная координата X окна
        //ymin - Минимальная координата Y окна
        public static Vector4 TransformToViewportMatrix(Vector4 vertex, float width, float height, float xmin, float ymin)
        {
            return Transform(vertex, Matrix4x4.CreateViewport(0, 0, width, height, xmin, ymin));
        }

        public static Vector4 Transform(Vector4 vertex, Matrix4x4 matrix)
        {
            var vector4 = new Vector4(vertex.X, vertex.Y, vertex.Z, vertex.W);
            var transformedV = Vector4.Transform(vector4, matrix);
            return new(transformedV.X, transformedV.Y, transformedV.Z, transformedV.W);
        }
    }
}