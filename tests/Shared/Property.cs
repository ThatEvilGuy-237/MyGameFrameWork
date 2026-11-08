using System.Collections;
using System.Globalization;

namespace Evil.Engine.Testing;

internal static class Property
{
    private const int DefaultCases = 300;

    public static void ForAll<T>(string name, Func<Random, T> generate, Action<T> check)
    {
        int seed = SeedFor(name);
        int cases = CaseCount();
        Random random = new Random(seed);
        for (int i = 0; i < cases; i++)
        {
            T input = generate(random);
            try
            {
                check(input);
            }
            catch (Exception failure)
            {
                string reason = failure is CheckFailed ? failure.Message : $"{failure.GetType().Name}: {failure.Message}";
                throw new CheckFailed($"{name}: case {i + 1} of {cases} (seed {seed}) failed for {Show(input)}: {reason}");
            }
        }
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

    private static string Show(object? input) => input switch
    {
        null => "null",
        string text => text,
        IFormattable value => value.ToString(null, CultureInfo.InvariantCulture),
        IEnumerable items => "[" + string.Join(", ", items.Cast<object?>().Select(Show)) + "]",
        _ => input.ToString() ?? "?",
    };
}
