using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGameFrameWork.Framework.Utils.DrawingShapes
{
    public struct Shape
    {
        public List<Vector3> Vertices { get; set; }
        public Vector4 SourceRect { get; set; }
        public Shape()
        {
            Vertices = new List<Vector3>();
            SourceRect = new Vector4(0, 0, 1, 1);
        }

        public Shape(List<Vector3> vertices, Vector4 sourceRect)
        {
            this.Vertices = vertices;
            this.SourceRect = sourceRect; 
        }
        public float[] GetFloatArray()
        {
            int vertexCount = Vertices.Count;

            if (vertexCount < 3)
            {
                return new float[0];
            }

            // If the vertex count is not a multiple of 3, duplicate the first point until the count is a multiple of 3
            while (vertexCount % 3 != 0)
            {
                Vertices.Add(Vertices[0]);
                vertexCount++;
            }

            // Now, create the float[] array. Each vertex has 3 components (x, y, z), no texture coordinates
            float[] result = new float[vertexCount * 5]; 

            // Calculate the texture coordinates based on the full size (0 to 1)
            float texWidth = SourceRect.Z;
            float texHeight = SourceRect.W;
            float texX = SourceRect.X;
            float texY = SourceRect.Y; 

            // Populate the result array with both vertices and texture coordinates
            for (int i = 0; i < vertexCount; i++)
            {
                result[i * 5] = Vertices[i].X;
                result[i * 5 + 1] = Vertices[i].Y;
                result[i * 5 + 2] = Vertices[i].Z; 

                // Texture coordinates for top-right corner mapping
                result[i * 5 + 3] = texX + (texWidth);
                result[i * 5 + 4] = texY + (texHeight);
            }

            return result;
        }


        public void ExtendShape(Shape other)
        {
            Vertices.AddRange(other.Vertices);
        }
        public void Add(Vector3 vector)
        {
            Vertices.Add(vector);
        }
        public void Add(Vector2 vector)
        {
            Vertices.Add(new Vector3(vector.X,vector.Y,0));
        }
        public static bool operator ==(Shape a, Shape b)
        {
            if (a.Vertices.Count != b.Vertices.Count) return false;

            for (int i = 0; i < a.Vertices.Count; i++)
            {
                if (!a.Vertices[i].Equals(b.Vertices[i])) return false;
            }

            return true;
        }
        public static bool operator !=(Shape a, Shape b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is Shape other)
            {
                return this == other;
            }
            return false;
        }
    }
}
