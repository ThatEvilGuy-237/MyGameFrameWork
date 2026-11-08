using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Evil.Engine.Testing;

internal sealed class MethodTest(MethodInfo method) : TestCase
{
    public override string Name => $"{method.DeclaringType!.Name}.{method.Name}";

    public override void Execute()
    {
        try
        {
            method.Invoke(null, null);
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(error.InnerException).Throw();
        }
    }
}
