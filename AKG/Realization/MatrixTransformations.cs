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
        public static Matrix4x4 GetWordMatrix( float ShiftX, float ShiftY, float ShiftZ,
            float RotateX, float RotateY, float RotateZ, float scale)
        {
           return CreateTranslation(ShiftX, ShiftY, ShiftZ) // матрица для сдвига
                            * CreateRotationX(RotateX) * CreateRotationY(RotateY) *
                            CreateRotationZ(RotateZ) // матрицы для поворотов
                            * CreateScale(scale); // матрица для масштаба
            
        }

        // Создает матрицу, которая преобразует координаты из мирового пространства в пространство наблюдателя.
        // eye - Позиция камеры (где она находится).
        // target - Позиция цели (куда смотрит камера).
        // up - Вектор "вверх" (обычно (0,1,0))
        public static Matrix4x4 GetViewMatrix( Vector3 eye, Vector3 target, Vector3 up)
        {
            return Matrix4x4.CreateLookAt(eye, target, up);
        }

        public static Matrix4x4 GetPerspectiveProjectionMatrix(float fov, float aspect, 
            float zNear, float zFar)
        {
           return Matrix4x4.CreatePerspectiveFieldOfView(
                fov, aspect, zFar, zNear);
           
        }

        // Создает матрицу преобразования в пространство окна просмотра (viewport).
        //width - Ширина окна просмотра
        //height - Высота окна просмотра
        //xmin - Минимальная координата X окна
        //ymin - Минимальная координата Y окна
        public static Matrix4x4 GetViewportMatrix( float width, float height, float xmin, float ymin)
        {
            return Matrix4x4.CreateViewport(0, 0, width, height, xmin, ymin);
        }

        public static Vector4 Transform(Vector4 vertex, Matrix4x4 matrix)
        {
            Vector4 res = Vector4.Transform(vertex, matrix);
            var w = res.W;
            res /= w;
            res.W = w;
            return res;
        }
    }
}