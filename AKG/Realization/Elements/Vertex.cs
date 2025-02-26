using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKG.Realization.Elements
{
    public class Vertex
    {
        public float X, Y, Z, W = 1.0f;

        public Vertex(float x, float y, float z, float w = 1.0f)
        {
            X = x; Y = y; Z = z; W = w;
        }
    }
}
