using MyGameFrameWork.Framework.Utils.Structs;
using OpenTK.Mathematics;

namespace MyGameFrameWork.Framework.Utils.DrawingShapes
{
    public struct Shape
    {
        public List<Vector3> Vertices { get; set; }
        public Shape()
        {
            Vertices = new List<Vector3>();
        }

        public Shape(List<Vector3> vertices)
        {
            this.Vertices = vertices;
        }
        public float[] GetFloatArray()
        {
            int vertexCount = Vertices.Count;
            if (vertexCount < 3)
            {
                return Array.Empty<float>(); // Not enough points for a valid shape
            }

            // Use a temporary list to avoid modifying the original shape
            List<Vector3> tempVertices = new List<Vector3>(Vertices);

            // Ensure vertex count is a multiple of 3 for proper rendering
            while (tempVertices.Count % 3 != 0)
            {
                tempVertices.Add(tempVertices[0]);
            }

            vertexCount = tempVertices.Count;
            float[] result = new float[vertexCount * 3]; // 3 for position, no need for texture coords here

            // Assign vertex position data
            for (int i = 0; i < vertexCount; i++)
            {
                result[i * 3] = tempVertices[i].X; // X position
                result[i * 3 + 1] = tempVertices[i].Y; // Y position
                result[i * 3 + 2] = tempVertices[i].Z; // Z position
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

        public RectF GetBoundingRectangle()
        {
            if (Vertices.Count == 0)
                return new RectF();

            // Initialize the bounding box to the first vertex
            float minX = Vertices[0].X;
            float maxX = Vertices[0].X;
            float minY = Vertices[0].Y;
            float maxY = Vertices[0].Y;

            foreach (var vertex in Vertices)
            {
                minX = Math.Min(minX, vertex.X);
                maxX = Math.Max(maxX, vertex.X);
                minY = Math.Min(minY, vertex.Y);
                maxY = Math.Max(maxY, vertex.Y);
            }

            return new RectF(minX, minY, maxX - minX, maxY - minY);
        }
        public void CenterAroundOrigin()
        {
            // Calculate the center of the shape
            float centerX = 0, centerY = 0;
            foreach (var vertex in Vertices)
            {
                centerX += vertex.X;
                centerY += vertex.Y;
            }

            centerX /= Vertices.Count;
            centerY /= Vertices.Count;

            // Translate all vertices by subtracting the center to center them around (0,0)
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = new Vector3(Vertices[i].X - centerX, Vertices[i].Y - centerY, Vertices[i].Z);
            }
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
