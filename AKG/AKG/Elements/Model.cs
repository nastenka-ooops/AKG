namespace AKG.Elements;

public class Model
{
    private readonly List<Vertex> _modelVertices;
    private readonly List<TextureCoordinate> _modelTextureCoordinates;
    private readonly List<Normal> _modelNormals;
    private readonly List<Face> _modelFaces;

    public Model(List<Vertex> modelVertices, List<TextureCoordinate> modelTextureCoordinates, List<Normal> modelNormals,
        List<Face> modelFaces)
    {
        _modelVertices = modelVertices;
        _modelTextureCoordinates = modelTextureCoordinates;
        _modelNormals = modelNormals;
        _modelFaces = modelFaces;
    }

    public List<Vertex> GetModelVertices() => _modelVertices;
    public List<TextureCoordinate> GetModelTextureCoordinates() => _modelTextureCoordinates;
    public List<Normal> GetModelNormals() => _modelNormals;
    public List<Face> GetModelFaces() => _modelFaces;
}