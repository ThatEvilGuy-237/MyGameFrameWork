using Evil.Engine.Input;

namespace Evil.Engine.GodotTests.Input;

internal static class ButtonStatesProperties
{
    private static readonly MouseButton[] Watched = [MouseButton.Left, MouseButton.Right, MouseButton.Middle];

    [Test]
    public static void ButtonsFollowTheirWholeHistory() =>
        Property.ForAll(nameof(ButtonsFollowTheirWholeHistory), ButtonStep.RandomScript, Replay);

    private static void Replay(ButtonStep[] script)
    {
        ButtonStates<MouseButton> states = new ButtonStates<MouseButton>();
        ButtonHistory history = new ButtonHistory();
        for (int i = 0; i < script.Length; i++)
        {
            script[i].ApplyTo(states);
            history.Add(script[i]);
            foreach (MouseButton button in Watched)
            {
                Check.Equal(history.IsDown(button), states.IsDown(button), $"{button} down after step {i + 1}");
                Check.Equal(history.WasPressed(button), states.WasPressed(button), $"{button} pressed after step {i + 1}");
                Check.Equal(history.WasReleased(button), states.WasReleased(button), $"{button} released after step {i + 1}");
            }
        }
    }
}
