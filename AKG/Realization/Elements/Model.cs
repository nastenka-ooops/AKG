using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using AKG.Realization;


namespace AKG.Realization.Elements
{
    public class Model
    {
        private readonly List<Vertex> _modelVertices;
        private readonly List<Vertex> _worldVertices;
        private readonly List<Vertex> _viewVertices;
        private readonly List<Vertex> _perspectiveVertices;
        private readonly List<Vertex> _viewportVertices;
        private readonly List<TextureCoordinate> _modelTextureCoordinates;
        private readonly List<Normal> _modelNormals;
        private readonly List<Face> _modelFaces;

        public float ShiftX { get; set; } = 0;
        public float ShiftY { get; set; } = 0;
        public float ShiftZ { get; set; } = 0;

        public float RotationOfXInRadians { get; set; } = 0;
        public float RotationOfYInRadians { get; set; } = 0;
        public float RotationOfZInRadians { get; set; } = 0;

        public float Scale { get; set; } = 1;
        public Vector3 eye;
        public Vector3 target;
        public Vector3 up;
        public float zFar = 100;
        public float zNear;
        public float Fov = (float)(20 * PI / 180);

        public Model(List<Vertex> modelVertices, List<TextureCoordinate> modelTextureCoordinates,
            List<Normal> modelNormals,
            List<Face> modelFaces)
        {
            _modelVertices = modelVertices;
            _modelTextureCoordinates = modelTextureCoordinates;
            _modelNormals = modelNormals;
            _modelFaces = modelFaces;
            _worldVertices = new(modelVertices);
            _viewVertices = new(modelVertices);
            _perspectiveVertices = new(modelVertices);
            _viewportVertices = new(modelVertices);
        }

        public void UpdateModelInfo(Vector3 eye, Vector3 target, Vector3 up)
        {
            this.eye = eye;
            this.target = target;
            this.up = up;
        }

        public List<Vertex> GetModelVertices() => _modelVertices;
        public List<Vertex> GetViewPortVertices() => _viewportVertices;
        public List<TextureCoordinate> GetModelTextureCoordinates() => _modelTextureCoordinates;
        public List<Normal> GetModelNormals() => _modelNormals;
        public List<Face> GetModelFaces() => _modelFaces;

        public void CalculateVertices(int width, int height)
        {
            float aspect = width / (float) height;
            zNear = width;

            var res = Parallel.For(0, _modelVertices.Count, i =>
            {
                _worldVertices[i] = MatrixTransformations.TransformToWordMatrix(_modelVertices[i], ShiftX, ShiftY,
                    ShiftZ,
                    RotationOfXInRadians, RotationOfYInRadians, RotationOfZInRadians, Scale);
                _viewVertices[i] = MatrixTransformations.TransformToViewMatrix(_worldVertices[i], eye, target, up);
                _perspectiveVertices[i] = MatrixTransformations.TransformToPerspectiveProjectionMatrix(
                    _viewVertices[i], Fov, aspect, zNear, zFar);
                _viewportVertices[i] =
                    MatrixTransformations.TransformToViewportMatrix(_perspectiveVertices[i], width, height, 0, 0);
            });
        }
    }
}