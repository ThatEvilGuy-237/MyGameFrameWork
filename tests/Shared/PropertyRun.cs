using System.Collections;
using System.Globalization;
using System.Text;

namespace Evil.Engine.Testing;

internal sealed class PropertyRun
{
    private const int DefaultCases = 300;

    public PropertyRun(string name)
    {
        Name = name;
        Seed = SeedFor(name);
        Cases = CaseCount();
        Random = new Random(Seed);
    }

    public string Name { get; }

    public int Seed { get; }

    public int Cases { get; }

    public Random Random { get; }

    public CheckFailed Failed(int index, object? sample, Exception failure)
    {
        string reason = failure is CheckFailed ? failure.Message : $"{failure.GetType().Name}: {failure.Message}";
        return new CheckFailed($"{Name}: case {index + 1} of {Cases} (seed {Seed}) failed for {Show(sample)}: {reason}");
    }

    private static int SeedFor(string name)
    {
        uint hash = 2166136261;
        foreach (char letter in name)
        {
            hash = (hash ^ letter) * 16777619;
        }

        string? salt = Environment.GetEnvironmentVariable("EVIL_PROPERTY_SEED");
        int extra = salt is null ? 0 : int.Parse(salt, CultureInfo.InvariantCulture);
        return (int)(hash & 0x7FFFFFFF) ^ extra;
    }

    private static int CaseCount()
    {
        string? given = Environment.GetEnvironmentVariable("EVIL_PROPERTY_CASES");
        return int.TryParse(given, NumberStyles.Integer, CultureInfo.InvariantCulture, out int cases) && cases > 0 ? cases : DefaultCases;
    }

    private static string Show(object? sample)
    {
        if (sample is null)
        {
            return "null";
        }

        if (sample is string or IFormattable || sample is not IEnumerable items)
        {
            return sample is IFormattable value ? value.ToString(null, CultureInfo.InvariantCulture) : sample.ToString() ?? "?";
        }

        StringBuilder shown = new StringBuilder("[");
        foreach (object? item in items)
        {
            shown.Append(shown.Length > 1 ? ", " : string.Empty);
            shown.Append(Show(item));
        }

        return shown.Append(']').ToString();
    }
}
