namespace Evil.Engine.Testing;

internal static class Property
{
    public static void ForAll<T>(string name, Func<Random, T> generate, Action<T> check)
    {
        PropertyRun run = new PropertyRun(name);
        for (int i = 0; i < run.Cases; i++)
        {
            T sample = generate(run.Random);
            try
            {
                check(sample);
            }
            catch (Exception failure)
            {
                throw run.Failed(i, sample, failure);
            }
        }
    }
}
