namespace AKG.Elements;

public class TextureCoordinate
{
    public float U, V = 0, W = 0;

    public TextureCoordinate(float u, float v = 0, float w = 0)
    {
        U = u; V = v; W = w;
    }
}