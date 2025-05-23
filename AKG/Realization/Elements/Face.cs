﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKG.Realization.Elements
{
    public class Face
    {
        public List<(int VertexIndex, int TextureIndex, int NormalIndex)> Indices = new List<(int VertexIndex, int TextureIndex, int NormalIndex)>();

        public Face(string[] faceParts)
        {
            foreach (var part in faceParts)
            {
                var indices = part.Split('/');
                int vIdx = int.Parse(indices[0], CultureInfo.InvariantCulture);
                int vtIdx = indices.Length > 1 && indices[1] != "" ? int.Parse(indices[1]) : -1;
                int vnIdx = indices.Length > 2 ? int.Parse(indices[2]) : -1;
                
                Indices.Add((vIdx, vtIdx, vnIdx));
            }
        }
    }
}
