using System.Numerics;

namespace Evil.Engine.Mathematics;

internal sealed class EarClipper
{
    private const float Epsilon = 1e-6f;

    private readonly Vector2[] _points;
    private readonly List<int> _remaining;
    private readonly List<int> _triangles;

    public EarClipper(ReadOnlySpan<Vector2> points)
    {
        _points = points.ToArray();
        _remaining = new List<int>(_points.Length);
        for (int i = 0; i < _points.Length; i++)
        {
            _remaining.Add(i);
        }

        if (Triangulator.SignedArea(points) < 0f)
        {
            _remaining.Reverse();
        }

        _triangles = new List<int>(Math.Max(0, (_points.Length - 2) * 3));
    }

    public int[] Run()
    {
        while (_remaining.Count > 3)
        {
            if (!TryClipEar() && !TryDropCollinear())
            {
                AddFan();
                return [.. _triangles];
            }
        }

        _triangles.AddRange(_remaining);
        return [.. _triangles];
    }

    private bool TryClipEar()
    {
        for (int at = 0; at < _remaining.Count; at++)
        {
            Corner corner = CornerAt(at);
            if (IsEar(corner))
            {
                _triangles.Add(corner.Previous);
                _triangles.Add(corner.Current);
                _triangles.Add(corner.Next);
                _remaining.RemoveAt(at);
                return true;
            }
        }

        return false;
    }

    private bool TryDropCollinear()
    {
        for (int at = 0; at < _remaining.Count; at++)
        {
            if (MathF.Abs(Turn(CornerAt(at))) <= Epsilon)
            {
                _remaining.RemoveAt(at);
                return true;
            }
        }

        return false;
    }

    private bool IsEar(Corner corner) => Turn(corner) > Epsilon && !HoldsAnotherCorner(corner);

    private bool HoldsAnotherCorner(Corner corner)
    {
        Vector2 a = _points[corner.Previous];
        Vector2 b = _points[corner.Current];
        Vector2 c = _points[corner.Next];
        foreach (int index in _remaining)
        {
            Vector2 point = _points[index];
            bool isCorner = index == corner.Previous || index == corner.Current || index == corner.Next;
            bool sitsOnCorner = point == a || point == b || point == c;
            if (!isCorner && !sitsOnCorner && InTriangle(point, a, b, c))
            {
                return true;
            }
        }

        return false;
    }

    private void AddFan()
    {
        for (int i = 1; i < _remaining.Count - 1; i++)
        {
            _triangles.Add(_remaining[0]);
            _triangles.Add(_remaining[i]);
            _triangles.Add(_remaining[i + 1]);
        }
    }

    private Corner CornerAt(int at)
    {
        int count = _remaining.Count;
        return new Corner(_remaining[(at + count - 1) % count], _remaining[at], _remaining[(at + 1) % count]);
    }

    private float Turn(Corner corner)
    {
        Vector2 a = _points[corner.Previous];
        Vector2 b = _points[corner.Current];
        Vector2 c = _points[corner.Next];
        return Triangulator.Cross(b - a, c - b);
    }

    private static bool InTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c) =>
        Triangulator.Cross(b - a, point - a) >= 0f
        && Triangulator.Cross(c - b, point - b) >= 0f
        && Triangulator.Cross(a - c, point - c) >= 0f;
}
