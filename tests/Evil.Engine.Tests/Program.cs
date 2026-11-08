using Evil.Engine.Testing;

namespace Evil.Engine.Tests;

internal static class Program
{
    private static int Main(string[] args) =>
        TestRunner.Run(typeof(Program).Assembly, "tests", args.FirstOrDefault());
}
