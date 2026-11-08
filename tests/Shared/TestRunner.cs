using System.Reflection;

namespace Evil.Engine.Testing;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class TestAttribute : Attribute;

internal static class TestRunner
{
    public static int Run(Assembly assembly, string suite, string? filter)
    {
        var tests = assembly.GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(method => method.GetCustomAttribute<TestAttribute>() is not null)
            .Select(method => (Name: $"{method.DeclaringType!.Name}.{method.Name}", Method: method))
            .Where(test => filter is null || test.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(test => test.Name, StringComparer.Ordinal)
            .ToList();

        var failed = 0;
        foreach (var (name, method) in tests)
        {
            try
            {
                method.Invoke(null, null);
                Console.WriteLine($"pass  {name}");
            }
            catch (TargetInvocationException error)
            {
                failed++;
                Console.WriteLine($"FAIL  {name}: {error.InnerException?.GetType().Name}: {error.InnerException?.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{tests.Count - failed} of {tests.Count} {suite} passed");
        return failed == 0 && tests.Count > 0 ? 0 : 1;
    }
}
