namespace Evil.Engine.Testing;

internal abstract class Property<TCase> : TestCase
{
    public override string Name => $"{Area}.{GetType().Name}";

    private string Area
    {
        get
        {
            string space = GetType().Namespace ?? string.Empty;
            return space[(space.LastIndexOf('.') + 1)..];
        }
    }

    public override void Execute()
    {
        PropertyRun run = new PropertyRun(GetType().Name);
        for (int i = 0; i < run.Cases; i++)
        {
            TCase sample = Generate(run.Random);
            try
            {
                Verify(sample);
            }
            catch (Exception failure)
            {
                throw run.Failed(i, sample, failure);
            }
        }
    }

    protected abstract TCase Generate(Random random);

    protected abstract void Verify(TCase sample);
}
