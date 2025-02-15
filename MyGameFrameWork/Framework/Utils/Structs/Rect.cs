using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGameFrameWork.Framework.Utils.Structs
{
    public interface IRectangle<T>
    {
        T X { get; }
        T Y { get; }
        T Width { get; }
        T Height { get; }
    }

    public class Rect : IRectangle<int>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public static Rect operator +(Rect rect1, Rect rect2)
        {
            return new Rect(
                rect1.X + rect2.X,
                rect1.Y + rect2.Y,
                rect1.Width + rect2.Width,
                rect1.Height + rect2.Height
            );
        }

        public static Rect operator -(Rect rect1, Rect rect2)
        {
            return new Rect(
                rect1.X - rect2.X,
                rect1.Y - rect2.Y,
                rect1.Width - rect2.Width,
                rect1.Height - rect2.Height
            );
        }

        public static Rect operator *(Rect rect, int scalar)
        {
            return new Rect(
                rect.X * scalar,
                rect.Y * scalar,
                rect.Width * scalar,
                rect.Height * scalar
            );
        }

        public static Rect operator /(Rect rect, int scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Scalar cannot be zero.");

            return new Rect(
                rect.X / scalar,
                rect.Y / scalar,
                rect.Width / scalar,
                rect.Height / scalar
            );
        }

        public override bool Equals(object obj)
        {
            if (obj is Rect other)
            {
                return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        public override string ToString()
        {
            return $"Rect(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";
        }

        public static Rect Unit()
        {
            return new Rect(0, 0, 1, 1);
        }
        public bool IsValid()
        {
            return Width > 0 && Height > 0;
        }
    }

    public class RectF : IRectangle<float>
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public RectF()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }
        public RectF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public static RectF operator +(RectF rect1, RectF rect2)
        {
            return new RectF(
                rect1.X + rect2.X,
                rect1.Y + rect2.Y,
                rect1.Width + rect2.Width,
                rect1.Height + rect2.Height
            );
        }

        public static RectF operator -(RectF rect1, RectF rect2)
        {
            return new RectF(
                rect1.X - rect2.X,
                rect1.Y - rect2.Y,
                rect1.Width - rect2.Width,
                rect1.Height - rect2.Height
            );
        }

        public static RectF operator *(RectF rect, float scalar)
        {
            return new RectF(
                rect.X * scalar,
                rect.Y * scalar,
                rect.Width * scalar,
                rect.Height * scalar
            );
        }
        public static RectF operator /(RectF rect, float scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Scalar cannot be zero.");

            return new RectF(
                rect.X / scalar,
                rect.Y / scalar,
                rect.Width / scalar,
                rect.Height / scalar
            );
        }

        public override bool Equals(object obj)
        {
            if (obj is RectF other)
            {
                return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        public override string ToString()
        {
            return $"RectF(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";
        }

        public static RectF Unit()
        {
            return new RectF(0, 0, 1, 1);
        }

        public bool IsValid()
        {
            return Width > 0 && Height > 0;
        }
    }

    public class RectD : IRectangle<double>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public RectD(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public static RectD operator +(RectD rect1, RectD rect2)
        {
            return new RectD(
                rect1.X + rect2.X,
                rect1.Y + rect2.Y,
                rect1.Width + rect2.Width,
                rect1.Height + rect2.Height
            );
        }

        public static RectD operator -(RectD rect1, RectD rect2)
        {
            return new RectD(
                rect1.X - rect2.X,
                rect1.Y - rect2.Y,
                rect1.Width - rect2.Width,
                rect1.Height - rect2.Height
            );
        }
        public static RectD operator *(RectD rect, double scalar)
        {
            return new RectD(
                rect.X * scalar,
                rect.Y * scalar,
                rect.Width * scalar,
                rect.Height * scalar
            );
        }

        public static RectD operator /(RectD rect, double scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Scalar cannot be zero.");

            return new RectD(
                rect.X / scalar,
                rect.Y / scalar,
                rect.Width / scalar,
                rect.Height / scalar
            );
        }

        public override bool Equals(object obj)
        {
            if (obj is RectD other)
            {
                return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        public override string ToString()
        {
            return $"RectD(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";
        }

        public static RectD Unit()
        {
            return new RectD(0, 0, 1, 1);
        }

        public bool IsValid()
        {
            return Width > 0 && Height > 0;
        }
    }
}

