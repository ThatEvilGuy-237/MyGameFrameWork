using Evil.Engine.Testing;

namespace Evil.Engine.Tests;

internal static class Program
{
    private static int Main(string[] args) =>
        new TestRunner(typeof(Program).Assembly, "tests").Run(args.Length > 0 ? args[0] : null);
}
