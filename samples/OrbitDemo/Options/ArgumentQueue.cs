using System.Diagnostics.CodeAnalysis;

namespace OrbitDemo;

internal sealed class ArgumentQueue(IReadOnlyList<string> arguments)
{
    private int _next;

    public bool TryTake([NotNullWhen(true)] out string? argument)
    {
        argument = _next < arguments.Count ? arguments[_next++] : null;
        return argument is not null;
    }

    public string ValueFor(string flag) =>
        TryTake(out string? value) ? value : throw new ArgumentException($"{flag} needs a value after it.");
}
