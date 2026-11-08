using System.Reflection;

namespace Evil.Engine.Testing;

internal sealed class TestRunner(Assembly assembly, string suite)
{
    private const BindingFlags Statics = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    public int Run(string? filter)
    {
        List<TestCase> tests = Find(filter);
        int failed = 0;
        foreach (TestCase test in tests)
        {
            failed += Passes(test) ? 0 : 1;
        }

        Console.WriteLine();
        Console.WriteLine($"{tests.Count - failed} of {tests.Count} {suite} passed");
        return failed == 0 && tests.Count > 0 ? 0 : 1;
    }

    private static bool Passes(TestCase test)
    {
        try
        {
            test.Execute();
            Console.WriteLine($"pass  {test.Name}");
            return true;
        }
        catch (Exception error)
        {
            Console.WriteLine($"FAIL  {test.Name}: {error.GetType().Name}: {error.Message}");
            return false;
        }
    }

    private static bool Matches(TestCase test, string? filter) =>
        filter is null || test.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);

    private static int ByName(TestCase first, TestCase second) => string.CompareOrdinal(first.Name, second.Name);

    private List<TestCase> Find(string? filter)
    {
        List<TestCase> found = new List<TestCase>();
        foreach (Type type in assembly.GetTypes())
        {
            foreach (TestCase test in TestsIn(type))
            {
                if (Matches(test, filter))
                {
                    found.Add(test);
                }
            }
        }

        found.Sort(ByName);
        return found;
    }

    private static List<TestCase> TestsIn(Type type)
    {
        List<TestCase> tests = new List<TestCase>();
        foreach (MethodInfo method in type.GetMethods(Statics))
        {
            if (method.GetCustomAttribute<TestAttribute>() is not null)
            {
                tests.Add(new MethodTest(method));
            }
        }

        if (!type.IsAbstract && type.IsSubclassOf(typeof(TestCase)) && type.GetConstructor(Type.EmptyTypes) is not null)
        {
            tests.Add((TestCase)Activator.CreateInstance(type)!);
        }

        return tests;
    }
}
