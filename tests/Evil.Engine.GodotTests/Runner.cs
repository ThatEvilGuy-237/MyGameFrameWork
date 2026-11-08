namespace Evil.Engine.GodotTests;

public partial class Runner : Godot.Node
{
    private int _code = 2;
    private int _framesAfterTests;

    public override void _Ready()
    {
        try
        {
            Stage.Host = this;
            string[] arguments = Godot.OS.GetCmdlineUserArgs();
            _code = new TestRunner(typeof(Runner).Assembly, "Godot tests").Run(arguments.Length > 0 ? arguments[0] : null);
        }
        catch (Exception error)
        {
            Console.WriteLine($"FAIL  the runner itself: {error}");
        }
    }

    public override void _Process(double delta)
    {
        _framesAfterTests++;
        if (_framesAfterTests == 2)
        {
            GetTree().Quit(_code);
        }
    }
}
