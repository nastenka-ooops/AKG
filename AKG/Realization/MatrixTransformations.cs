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
        public static Matrix4x4 CreateWordMatr(float ShiftX, float ShiftY, float ShiftZ, float RotateX, float RotateY, float RotateZ, float scale)
        {
            var worldMatr = CreateTranslation(ShiftX, ShiftY, ShiftZ)                            // матрица для сдвига
                * CreateRotationX(RotateX) * CreateRotationY(RotateY) * CreateRotationZ(RotateZ) // матрицы для поворотов
                * CreateScale(scale);                                                            // матрица для масштаба       
            return worldMatr;
        }

        public static Vertex TransformToWord(Vertex v, Matrix4x4 wordMatr)
        {
            var vector4 = new Vector4(v.X, v.Y, v.Z, v.W);
            var transformedV = Vector4.Transform(vector4, wordMatr);
            return new(transformedV.X, transformedV.Y, transformedV.Z, transformedV.W);
        }
    }
}