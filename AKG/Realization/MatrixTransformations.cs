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
        public static Matrix4x4 CreateWordMatrix(float ShiftX, float ShiftY, float ShiftZ,
            float RotateX, float RotateY, float RotateZ, float scale)
        {
            var worldMatr = CreateTranslation(ShiftX, ShiftY, ShiftZ) // матрица для сдвига
                            * CreateRotationX(RotateX) * CreateRotationY(RotateY)
                        //  * CreateRotationZ(RotateZ) // матрицы для поворотов
                            * CreateScale(scale); // матрица для масштаба       
            return worldMatr;
        }

        public static Vertex TransformToWord(Vertex v, Matrix4x4 wordMatr)
        {
            var vector4 = new Vector4(v.X, v.Y, v.Z, v.W);
            var transformedV = Vector4.Transform(vector4, wordMatr);
            return new(transformedV.X, transformedV.Y, transformedV.Z, transformedV.W);
        }

        // Создает матрицу, которая преобразует координаты из мирового пространства в пространство наблюдателя.
        // eye - Позиция камеры (где она находится).
        // target - Позиция цели (куда смотрит камера).
        // up - Вектор "вверх" (обычно (0,1,0))
        public static Matrix4x4 CreateViewMatrix(Vector3 eye, Vector3 target, Vector3 up)
        {
            // Вычисляем оси камеры
            Vector3 zAxis = Vector3.Normalize(eye - target); // Ось Z (направление от камеры к цели)
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis)); // Ось X (перпендикуляр к Z и UP)
            Vector3 yAxis = up; // Ось Y (перпендикуляр к Z и X)

            return new Matrix4x4(
                xAxis.X, yAxis.X, zAxis.X, -Vector3.Dot(xAxis, eye),
                xAxis.Y, yAxis.Y, zAxis.Y, -Vector3.Dot(yAxis, eye),
                xAxis.Z, yAxis.Z, zAxis.Z, -Vector3.Dot(zAxis, eye),
                0, 0, 0, 1
            );
        }

        // Преобразует координаты из мирового пространства в пространство наблюдателя.
        // v - Вершина в мировых координатах
        // viewMatrix - Матрица вида 
        public static Vertex TransformToView(Vertex v, Matrix4x4 viewMatrix)
        {
            var vector4 = new Vector4(v.X, v.Y, v.Z, v.W);
            var transformedV = Vector4.Transform(vector4, viewMatrix);
            return new Vertex(transformedV.X, transformedV.Y, transformedV.Z, transformedV.W);
        }

        public static Matrix4x4 CreatePerspectiveProjectionMatrix(float fov, float aspect, float zNear, float zFar)
        {
            float m11 = (float)(1 / Math.Tan(fov / 2) / aspect);
            float m22 = (float)(1 / Math.Tan(fov / 2));
            float m33 = (float)(zFar / (zNear - zFar));
            float m34 = (float)(zFar * zNear / (zNear - zFar));

            return new Matrix4x4(
               m11, 0, 0, 0,
               0, m22, 0, 0,
               0, 0, m33, m34,
               0, 0, 1, 0
           );
        }

        public static Vertex TransformToPerspective(Vertex v, Matrix4x4 perspectiveMatr)
        {
            var vector4 = new Vector4(v.X, v.Y, v.Z, v.W);
            var transformedV = Vector4.Transform(vector4, perspectiveMatr);
            transformedV.X = transformedV.X/transformedV.W;
            transformedV.Y = transformedV.Y/transformedV.W;
            transformedV.Z = transformedV.Z/transformedV.W;
            transformedV.W = transformedV.W/transformedV.W;
            return new Vertex(transformedV.X, transformedV.Y, transformedV.Z, transformedV.W);
        }


        // Создает ортографическую проекционную матрицу.
        // width - Ширина области обзора
        // height - Высота области обзора
        // znear - Ближняя плоскость отсечения
        // zfar - Дальняя плоскость отсечения
        public static Matrix4x4 CreateOrthographicProjectionMatrix(float width, float height, float znear, float zfar)
        {
            return new Matrix4x4(
                2f / width, 0, 0, 0,
                0, 2f / height, 0, 0,
                0, 0, 1f / (znear - zfar), znear / (znear - zfar),
                0, 0, 0, 1
            );
        }

        // Преобразует вершину из пространства наблюдателя в пространство проекции.
        // v - Вершина в пространстве наблюдателя
        // projectionMatrix - Матрица проекции
        public static Vertex TransformToProjection(Vertex v, Matrix4x4 projectionMatrix)
        {
            var vector4 = new Vector4(v.X, v.Y, v.Z, v.W);
            var transformedV = Vector4.Transform(vector4, projectionMatrix);
            return new Vertex(transformedV.X, transformedV.Y, transformedV.Z, transformedV.W);
        }

        // Создает матрицу преобразования в пространство окна просмотра (viewport).
        //width - Ширина окна просмотра
        //height - Высота окна просмотра
        //xmin - Минимальная координата X окна
        //ymin - Минимальная координата Y окна
        public static Matrix4x4 CreateViewportMatrix(float width, float height, float xmin, float ymin)
        {
            
            return new Matrix4x4(
                width / 2f, 0, 0, xmin + width / 2f,
                0, -height / 2f, 0, ymin + height / 2f,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
        }

        // Преобразует вершину из пространства проекции в пространство окна просмотра.
        // v - Вершина в пространстве проекции
        // viewportMatrix - Матрица преобразования в пространство окна просмотра
        public static Vertex TransformToViewport(Vertex v, Matrix4x4 viewportMatrix)
        {
            var vector4 = new Vector4(v.X, v.Y, v.Z, v.W);
            var transformedV = Vector4.Transform(vector4, viewportMatrix);
            return new Vertex(transformedV.X, transformedV.Y, transformedV.Z, transformedV.W);
        }
    }
}