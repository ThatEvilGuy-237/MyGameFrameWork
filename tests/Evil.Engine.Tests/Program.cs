using System.Reflection;

namespace Evil.Engine.Tests;

internal static class Program
{
    private static int Main(string[] args)
    {
        var filter = args.FirstOrDefault();
        var tests = typeof(Program).Assembly.GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(method => method.GetCustomAttribute<TestAttribute>() is not null)
            .Select(method => (Name: $"{method.DeclaringType!.Name}.{method.Name}", Method: method))
            .Where(test => filter is null || test.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(test => test.Name, StringComparer.Ordinal)
            .ToList();

        var failed = new List<string>();
        foreach (var (name, method) in tests)
        {
            try
            {
                method.Invoke(null, null);
                Console.WriteLine($"pass  {name}");
            }
            catch (TargetInvocationException error)
            {
                failed.Add(name);
                Console.WriteLine($"FAIL  {name}: {error.InnerException?.GetType().Name}: {error.InnerException?.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{tests.Count - failed.Count} of {tests.Count} tests passed");
        return failed.Count == 0 && tests.Count > 0 ? 0 : 1;
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class TestAttribute : Attribute;
