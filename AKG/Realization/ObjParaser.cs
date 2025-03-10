using System.Globalization;
using System.Numerics;
using AKG.Realization.Elements;

namespace AKG.Realization
{
    public class ObjParser
    {
        public Model Parse(string filePath)
        {
            List<Vector4> Vertices = new List<Vector4>();
            List<TextureCoordinate> TextureCoordinates = new List<TextureCoordinate>();
            List<Normal> Normals = new List<Normal>();
            List<Face> Faces = new List<Face>();

            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v":
                        Vertices.Add(new Vector4(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture),
                            parts.Length > 4 ? float.Parse(parts[4], CultureInfo.InvariantCulture) : 1.0f));
                        break;

                    case "vt":
                        TextureCoordinates.Add(new TextureCoordinate(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            parts.Length > 2 ? float.Parse(parts[2], CultureInfo.InvariantCulture) : 0,
                            parts.Length > 3 ? float.Parse(parts[3], CultureInfo.InvariantCulture) : 0));
                        break;

                    case "vn":
                        Normals.Add(new Normal(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture)));
                        break;

                    case "f":
                        Faces.Add(new Face(parts[1..]));
                        break;
                }
            }
            return new Model(Vertices, TextureCoordinates, Normals, Faces);
        }
    }
}