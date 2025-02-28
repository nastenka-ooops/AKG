using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace AKG.Realization.Elements
{
    public class Model
    {
        private readonly List<Vertex> _modelVertices;
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
        public Vertex eye;
        public Vertex target;
        public Vertex up;
        public float zFar = 100;
        public Vertex zNear;
        public float Fov = (float)(20 * PI / 180);

        public Model(List<Vertex> modelVertices, List<TextureCoordinate> modelTextureCoordinates, List<Normal> modelNormals,
            List<Face> modelFaces)
        {
            _modelVertices = modelVertices;
            _modelTextureCoordinates = modelTextureCoordinates;
            _modelNormals = modelNormals;
            _modelFaces = modelFaces;
        }

        public void UpdateModelInfo(Vertex eye,Vertex target, Vertex up) 
        {
            this.eye = eye;
            this.target = target;
            this.up = up;
        }

        public List<Vertex> GetModelVertices() => _modelVertices;
        public List<TextureCoordinate> GetModelTextureCoordinates() => _modelTextureCoordinates;
        public List<Normal> GetModelNormals() => _modelNormals;
        public List<Face> GetModelFaces() => _modelFaces;
    }
}
