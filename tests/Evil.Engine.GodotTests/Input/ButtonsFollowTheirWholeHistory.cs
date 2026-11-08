using Evil.Engine.Input;

namespace Evil.Engine.GodotTests.Input;

internal sealed class ButtonsFollowTheirWholeHistory : Property<ButtonStep[]>
{
    private static readonly MouseButton[] Watched = [MouseButton.Left, MouseButton.Right, MouseButton.Middle];

    protected override ButtonStep[] Generate(Random random) => ButtonStep.RandomScript(random);

    protected override void Verify(ButtonStep[] sample)
    {
        ButtonStates<MouseButton> states = new ButtonStates<MouseButton>();
        ButtonHistory history = new ButtonHistory();
        for (int i = 0; i < sample.Length; i++)
        {
            sample[i].ApplyTo(states);
            history.Add(sample[i]);
            foreach (MouseButton button in Watched)
            {
                Check.Equal(history.IsDown(button), states.IsDown(button), $"{button} down after step {i + 1}");
                Check.Equal(history.WasPressed(button), states.WasPressed(button), $"{button} pressed after step {i + 1}");
                Check.Equal(history.WasReleased(button), states.WasReleased(button), $"{button} released after step {i + 1}");
            }
        }
    }
}
