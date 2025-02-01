using OpenTK.Mathematics;

namespace MyGameFrameWork.Framework.Utils
{

    #pragma warning disable CS0660
    #pragma warning disable CS0661
    public struct SRTVector
    {
        public Vector3 Scale;
        public Vector3 Rotate;
        public Vector3 Translate;
        public SRTVector(Vector3 scale, Vector3 rotate, Vector3 translate)
        {
            Scale = scale;
            Rotate = rotate;
            Translate = translate;
        }
        public SRTVector(SRTVector newVectors)
        {
            Scale = newVectors.Scale;
            Rotate = newVectors.Rotate;
            Translate = newVectors.Translate;
        }
        public SRTVector()
        {
            Scale = new Vector3(1, 1, 1);
            Rotate = new Vector3(0, 0, 0);
            Translate = new Vector3(0, 0, 0);
        }
        public static bool operator ==(SRTVector a, SRTVector b)
        {
            if (a.Scale == b.Scale && a.Rotate == b.Rotate && a.Translate == b.Translate)
            {
                return true;
            }

            return false;
        }
        public static bool operator !=(SRTVector a, SRTVector b)
        {
            return !(a == b);
        }
        public static SRTVector operator +(SRTVector a, SRTVector b)
        {
            a.Scale += b.Scale;
            a.Rotate += b.Rotate;
            a.Translate += b.Translate;

            return a;
        }
        public static SRTVector operator *(SRTVector a, SRTVector b)
        {
            a.Scale *= b.Scale;
            a.Rotate *= b.Rotate;
            a.Translate *= b.Translate;

            return a;
        }

        public Matrix4 GetSRTMatrix()
        {
            Matrix4 translationMatrix = CalcTranslationMatrix(Translate);
            Matrix4 rotationMatrix = CalcRotationMatrix(Rotate);
            Matrix4 scalingMatrix = CalcScalingMatrix(Scale);

            // Combine the matrices: Translation -> Rotation -> Scaling
            Matrix4 transformationMatrix = scalingMatrix * rotationMatrix * translationMatrix;

            return transformationMatrix;
        }
        public Matrix4 CalcTranslationMatrix(Vector3 translate)
        {
            return Matrix4.CreateTranslation(translate);
        }
        public Matrix4 CalcRotationMatrix(Vector3 rotate)
        {
            Matrix4 rotationMatrixX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotate.X));
            Matrix4 rotationMatrixY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotate.Y));
            Matrix4 rotationMatrixZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotate.Z));

            // Combine the rotations in the correct order: Z -> Y -> X
            return rotationMatrixZ * rotationMatrixY * rotationMatrixX;
        }
        public Matrix4 CalcScalingMatrix(Vector3 scale)
        {
            return Matrix4.CreateScale(scale);
        }
    }
    internal class SRTVectorsStack
    {
        Stack<SRTVector> m_StackVectors;

        public SRTVectorsStack()
        {
            m_StackVectors = new Stack<SRTVector>();
        }

        public void Add(Vector3 scale, Vector3 rotate, Vector3 translate)
        {
            m_StackVectors.Push(new SRTVector(scale, rotate, translate));
        }

        public void Add()
        {
            Add(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        }
        //updates all ements in list
        public void SetScale(Vector3 scale)
        {
            var updatedStack = new Stack<SRTVector>();
            foreach (var v in m_StackVectors)
            {
                updatedStack.Push(new SRTVector(scale, v.Rotate, v.Translate));
            }
            m_StackVectors = updatedStack;
        }

        public void SetRotate(Vector3 rotate)
        {
            var updatedStack = new Stack<SRTVector>();
            foreach (var v in m_StackVectors)
            {
                updatedStack.Push(new SRTVector(v.Scale, rotate, v.Translate));
            }
            m_StackVectors = updatedStack;
        }

        public void SetTranslate(Vector3 translate)
        {
            var updatedStack = new Stack<SRTVector>();
            foreach (var v in m_StackVectors)
            {
                updatedStack.Push(new SRTVector(v.Scale, v.Rotate, translate));
            }
            m_StackVectors = updatedStack;
        }

        public bool Exitst()
        {
            return m_StackVectors.Count > 0;
        }

        public SRTVector GetLast()
        {
            return GetLastSRTVector();
        }

        public Stack<SRTVector> GetList()
        {
            return m_StackVectors;
        }

        public SRTVector GetLastSRTVector()
        {
            if (!Exitst()) return new SRTVector();
            return m_StackVectors.Peek();
        }

        public Vector3 GetScale()
        {
            return GetLastSRTVector().Scale;
        }

        public Vector3 GetRotate()
        {
            return GetLastSRTVector().Rotate;
        }

        public Vector3 GetTranslate()
        {
            return GetLastSRTVector().Translate;
        }

        public void Pop()
        {
            if (Exitst())
                m_StackVectors.Pop();
        }
    }

}
