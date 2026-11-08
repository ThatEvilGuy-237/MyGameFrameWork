using Evil.Engine.Input;

namespace Evil.Engine.GodotTests.Input;

internal sealed class ButtonHistory
{
    private readonly List<ButtonStep> _steps = new List<ButtonStep>();

    public void Add(ButtonStep step) => _steps.Add(step);

    public bool IsDown(MouseButton button) => IsDownAfter(_steps.Count, button);

    public bool WasPressed(MouseButton button)
    {
        for (int i = _steps.Count - 1; i >= 0 && _steps[i].Action != ButtonAction.EndFrame; i--)
        {
            if (_steps[i].Action == ButtonAction.Press && _steps[i].Button == button)
            {
                return true;
            }
        }

        return false;
    }

    public bool WasReleased(MouseButton button)
    {
        for (int i = _steps.Count - 1; i >= 0 && _steps[i].Action != ButtonAction.EndFrame; i--)
        {
            bool releasedThis = _steps[i].Action == ButtonAction.Release && _steps[i].Button == button;
            bool releasedAllWhileDown = _steps[i].Action == ButtonAction.ReleaseAll && IsDownAfter(i, button);
            if (releasedThis || releasedAllWhileDown)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsDownAfter(int count, MouseButton button)
    {
        for (int i = count - 1; i >= 0; i--)
        {
            ButtonStep step = _steps[i];
            if (step.Action == ButtonAction.ReleaseAll || (step.Action == ButtonAction.Release && step.Button == button))
            {
                return false;
            }

            if (step.Action == ButtonAction.Press && step.Button == button)
            {
                return true;
            }
        }

        return false;
    }
}
