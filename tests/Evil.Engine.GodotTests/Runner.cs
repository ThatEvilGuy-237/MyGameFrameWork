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
            _code = TestRunner.Run(typeof(Runner).Assembly, "Godot tests", Godot.OS.GetCmdlineUserArgs().FirstOrDefault());
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
