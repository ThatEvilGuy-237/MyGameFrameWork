using System.Numerics;

namespace Evil.Engine.Testing;

internal static class Check
{
    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new CheckFailed(message);
        }
    }

    public static void Equal<T>(T expected, T actual, string what = "value")
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new CheckFailed($"{what}: expected {expected}, got {actual}");
        }
    }

    public static void Near(float expected, float actual, string what = "value", float tolerance = 1e-3f)
    {
        if (MathF.Abs(expected - actual) > tolerance)
        {
            throw new CheckFailed($"{what}: expected {expected}, got {actual}");
        }
    }

    public static void Near(Vector2 expected, Vector2 actual, string what = "point", float tolerance = 1e-3f)
    {
        if (Vector2.Distance(expected, actual) > tolerance)
        {
            throw new CheckFailed($"{what}: expected {expected}, got {actual}");
        }
    }

    public static void Near(Vector3 expected, Vector3 actual, string what = "point", float tolerance = 1e-3f)
    {
        if (Vector3.Distance(expected, actual) > tolerance)
        {
            throw new CheckFailed($"{what}: expected {expected}, got {actual}");
        }
    }

    public static TException Throws<TException>(Action action, string? messagePart = null)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException error)
        {
            if (messagePart is not null && !error.Message.Contains(messagePart, StringComparison.Ordinal))
            {
                throw new CheckFailed($"{typeof(TException).Name} message '{error.Message}' does not mention '{messagePart}'");
            }

            return error;
        }
        catch (Exception other)
        {
            throw new CheckFailed($"expected {typeof(TException).Name}, got {other.GetType().Name}: {other.Message}");
        }

        throw new CheckFailed($"expected {typeof(TException).Name}, nothing was thrown");
    }
}
