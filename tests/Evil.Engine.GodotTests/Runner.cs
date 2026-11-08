namespace Evil.Engine.GodotTests;

public partial class Runner : Godot.Node
{
    public override void _Ready()
    {
        var code = 2;
        try
        {
            Stage.Host = this;
            code = TestRunner.Run(typeof(Runner).Assembly, "Godot tests", Godot.OS.GetCmdlineUserArgs().FirstOrDefault());
        }
        catch (Exception error)
        {
            Console.WriteLine($"FAIL  the runner itself: {error}");
        }

        GetTree().Quit(code);
    }
}
