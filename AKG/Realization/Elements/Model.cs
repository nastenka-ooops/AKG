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
        private readonly List<Vector4> _modelVertices;
        private readonly List<Vector4> _worldVertices;
        private readonly List<Vector4> _viewVertices;
        private readonly List<Vector4> _perspectiveVertices;
        private readonly List<Vector4> _viewportVertices;
        private readonly List<Vector3> _modelTextureCoordinates;
        private readonly List<Normal> _modelNormals;
        private readonly List<Face> _modelFaces;

        public float ShiftX { get; set; } = 0;
        public float ShiftY { get; set; } = 0;
        public float ShiftZ { get; set; } = 0;

        public float RotationOfXInRadians { get; set; }
        public float RotationOfYInRadians { get; set; }
        public float RotationOfZInRadians { get; set; } = 0;

        public float Scale { get; set; } = 0.005f;

        public Vector3 Ka { get; set; }//фоновое
        public Vector3 Kd { get; set; }//рассеяное
        public Vector3 Ks { get; set; }//зеркальное
        public float Shininess { get; set; } // коэф блеска

        public Vector3 lightColor { get; set; } = new Vector3(0.9f, 0.9f, 0.9f);

        public Bitmap DiffuseMap { get; set; } = new Bitmap("../../../Models/diffuse-maps/bricks.jpg");
        public Bitmap NormalMap { get; set; } = new Bitmap("../../../Models/normal-maps/bricks.png");

        public void setDefaultMaterial()
        {
            this.Ka = new Vector3(0.1f, 0.1f, 0.1f);
            this.Kd = new Vector3(0.9f, 0.9f, 0.9f);
            this.Ks = new Vector3(0.5f, 0.5f, 0.5f);
            this.Shininess = 32f;
        }

        public Vector3 eye;
        public Vector3 target;
        public Vector3 up;
        public Vector3 lightDir = new Vector3(0, -1, 0);
        public float zFar = 100;
        public float zNear;
        public float Fov = (float)(20 * PI / 180);

        public Model(List<Vector4> modelVertices, List<Vector3> modelTextureCoordinates,
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

        public List<Vector4> GetModelVertices() => _modelVertices;
        public List<Vector4> GetWorldVertices() => _worldVertices;
        public List<Vector4> GetViewPortVertices() => _viewportVertices;
        public List<Vector3> GetModelTextureCoordinates() => _modelTextureCoordinates;
        public List<Normal> GetModelNormals() => _modelNormals;
        public List<Face> GetModelFaces() => _modelFaces;


        public void CalculateVertices(int width, int height)
        {
            float aspect = width / (float)height;
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


        public List<Vector3> GetNormals()
        {

            // Если нормалей нет, вычисляем их как усредненные нормали смежных граней
            var vertexNormals = new Dictionary<int, Vector3>();
            var vertexToFaces = new Dictionary<int, List<Vector3>>();

            // Собираем все грани, связанные с каждой вершиной
            foreach (var face in _modelFaces)
            {
                var v0 = face.Indices[0].VertexIndex - 1;
                var v1 = face.Indices[1].VertexIndex - 1;
                var v2 = face.Indices[2].VertexIndex - 1;

                // Вычисляем нормаль текущей грани
                var edge1 = _modelVertices[v1] - _modelVertices[v0];
                var edge2 = _modelVertices[v2] - _modelVertices[v0];
                var faceNormal = Vector3.Normalize(Vector3.Cross(
                    new Vector3(edge1.X, edge1.Y, edge1.Z),
                    new Vector3(edge2.X, edge2.Y, edge2.Z)));

                // Добавляем нормаль грани к каждой вершине
                for (int i = 0; i < 3; i++)
                {
                    var vertexIndex = face.Indices[i].VertexIndex - 1;
                    if (!vertexToFaces.ContainsKey(vertexIndex))
                        vertexToFaces[vertexIndex] = new List<Vector3>();
                    vertexToFaces[vertexIndex].Add(faceNormal);
                }
            }

            // Усредняем нормали для каждой вершины
            var normals = new List<Vector3>(_modelVertices.Count);
            for (int i = 0; i < _modelVertices.Count; i++)
            {
                if (vertexToFaces.TryGetValue(i, out var faceNormals))
                {
                    var avgNormal = Vector3.Zero;
                    foreach (var normal in faceNormals)
                        avgNormal += normal;
                    normals.Add(Vector3.Normalize(avgNormal));
                }
                else
                {
                    normals.Add(Vector3.UnitZ); // Дефолтная нормаль, если вершина не используется
                }
            }

            return normals;
        }
    }
}