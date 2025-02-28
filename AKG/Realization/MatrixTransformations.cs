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
        public static Vertex TransformToWordMatrix(Vertex vertex, float ShiftX, float ShiftY, float ShiftZ,
            float RotateX, float RotateY, float RotateZ, float scale)
        {
            var worldMatr = CreateTranslation(ShiftX, ShiftY, ShiftZ) // матрица для сдвига
                            * CreateRotationX(RotateX) * CreateRotationY(RotateY) * CreateRotationZ(RotateZ) // матрицы для поворотов
                            * CreateScale(scale); // матрица для масштаба       
            return Transform(vertex, worldMatr);
        }
        
        // Создает матрицу, которая преобразует координаты из мирового пространства в пространство наблюдателя.
        // eye - Позиция камеры (где она находится).
        // target - Позиция цели (куда смотрит камера).
        // up - Вектор "вверх" (обычно (0,1,0))
        public static Vertex TransformToViewMatrix(Vertex vertex, Vector3 eye, Vector3 target, Vector3 up)
        {
            // Вычисляем оси камеры
            Vector3 zAxis = Vector3.Normalize(eye - target); // Ось Z (направление от камеры к цели)
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis)); // Ось X (перпендикуляр к Z и UP)
            Vector3 yAxis = up; // Ось Y (перпендикуляр к Z и X)

            var viewMatr = new Matrix4x4(
                xAxis.X, xAxis.Y, xAxis.Z, -Vector3.Dot(xAxis, eye),
                yAxis.X, yAxis.Y, yAxis.Z, -Vector3.Dot(yAxis, eye),
                zAxis.X, zAxis.Y, zAxis.Z, -Vector3.Dot(zAxis, eye),
                0, 0, 0, 1
            );
            return Transform(vertex, viewMatr);
        }

        public static Vertex TransformToPerspectiveProjectionMatrix(Vertex vertex, float fov, float aspect, float zNear, float zFar)
        {
            float m11 = (float)(1 / Math.Tan(fov / 2) / aspect);
            float m22 = (float)(1 / Math.Tan(fov / 2));
            float m33 = (float)(zFar / (zNear - zFar));
            float m34 = (float)(zFar * zNear / (zNear - zFar));

            var perspectiveProjectionMatr =  new Matrix4x4(
               m11, 0, 0, 0,
               0, m22, 0, 0,
               0, 0, m33, m34,
               0, 0, -1, 0
            );
            var transformVertex = Transform(vertex, perspectiveProjectionMatr);
            return new Vertex(transformVertex.X/transformVertex.W, transformVertex.Y/transformVertex.W, 
                transformVertex.Z/transformVertex.W, transformVertex.W/transformVertex.W);
        }

        // Создает ортографическую проекционную матрицу.
        // width - Ширина области обзора
        // height - Высота области обзора
        // znear - Ближняя плоскость отсечения
        // zfar - Дальняя плоскость отсечения
        public static Vertex TransformToOrthographicProjectionMatrix(Vertex vertex, float width, float height, float znear, float zfar)
        {
            var orthographicProjectionMatr = new Matrix4x4(
                2f / width, 0, 0, 0,
                0, 2f / height, 0, 0,
                0, 0, 1f / (znear - zfar), znear / (znear - zfar),
                0, 0, 0, 1
            );
            return Transform(vertex, orthographicProjectionMatr);
        }

        // Создает матрицу преобразования в пространство окна просмотра (viewport).
        //width - Ширина окна просмотра
        //height - Высота окна просмотра
        //xmin - Минимальная координата X окна
        //ymin - Минимальная координата Y окна
        public static Vertex TransformToViewportMatrix(Vertex vertex, float width, float height, float xmin, float ymin)
        {
            
            var viewportMatr = new Matrix4x4(
                width / 2f, 0, 0, xmin + width / 2f,
                0, -height / 2f, 0, ymin + height / 2f,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
            return Transform(vertex, viewportMatr);
        }
        
        public static Vertex Transform(Vertex vertex, Matrix4x4 matrix)
        {
            var vector4 = new Vector4(vertex.X, vertex.Y, vertex.Z, vertex.W);
            var transformedV = Vector4.Transform(vector4, matrix);
            return new(transformedV.X, transformedV.Y, transformedV.Z, transformedV.W);
        }

    }
}