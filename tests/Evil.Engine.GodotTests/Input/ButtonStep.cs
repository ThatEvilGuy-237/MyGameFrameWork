using Evil.Engine.Input;

namespace Evil.Engine.GodotTests.Input;

internal sealed record ButtonStep(ButtonAction Action, MouseButton Button)
{
    private static readonly MouseButton[] Buttons = [MouseButton.Left, MouseButton.Right, MouseButton.Middle];

    public static ButtonStep[] RandomScript(Random random)
    {
        ButtonStep[] steps = new ButtonStep[random.Next(1, 40)];
        for (int i = 0; i < steps.Length; i++)
        {
            ButtonAction action = (ButtonAction)random.Next(4);
            steps[i] = new ButtonStep(action, Gen.Pick(random, Buttons));
        }

        return steps;
    }

    public void ApplyTo(ButtonStates<MouseButton> states)
    {
        if (Action == ButtonAction.Press)
        {
            states.Press(Button);
        }
        else if (Action == ButtonAction.Release)
        {
            states.Release(Button);
        }
        else if (Action == ButtonAction.ReleaseAll)
        {
            states.ReleaseAll();
        }
        else
        {
            states.EndFrame();
        }
    }
}
