using AKG;
using AKG.Elements;

class Program
{
    public static void Main(string[] args)
    {
        ObjParser parser = new ObjParser();
        Model model = parser.Parse("C:\\BSUIR\\AKGv2\\models\\hand-low-poly.obj");
        Console.WriteLine(model.GetModelVertices().Count);
    }
}