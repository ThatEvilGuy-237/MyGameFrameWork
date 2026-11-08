namespace OrbitDemo;

internal sealed class ActionBar
{
    private const float ButtonWidth = 150f;
    private const float ButtonHeight = 46f;
    private const float Gap = 16f;
    private const float BottomMargin = 32f;

    private static readonly string[] Actions = ["Move", "Attack", "Wait"];

    private Vector2 _loggedScreen;

    public string? Hovered { get; private set; }

    public void Update(Vector2 screen, Vector2 mouse, bool shown)
    {
        Hovered = null;
        if (!shown)
        {
            return;
        }

        LogLayout(screen);
        RectF[] buttons = Layout(screen);
        int index = Array.FindIndex(buttons, button => button.Contains(mouse));
        Hovered = index >= 0 ? Actions[index] : null;
    }

    public void Draw(Canvas canvas, bool locked)
    {
        RectF[] buttons = Layout(canvas.Size);
        for (int i = 0; i < buttons.Length; i++)
        {
            bool hovered = Actions[i] == Hovered && !locked;
            DrawButton(canvas, buttons[i], Actions[i], hovered, locked);
        }
    }

    private static void DrawButton(Canvas canvas, RectF box, string label, bool hovered, bool locked)
    {
        Color fill = hovered ? Color.FromHex("#3E63DD") : Color.Black.WithAlpha(locked ? 0.25f : 0.55f);
        canvas.FillRect(box, fill);
        canvas.DrawRect(box, Color.White.WithAlpha(hovered ? 0.9f : 0.35f), 2f);
        canvas.DrawText(label, box, 18f, Color.White.WithAlpha(locked ? 0.4f : 1f), TextAlign.Center);
    }

    private static RectF[] Layout(Vector2 screen)
    {
        float rowWidth = ButtonWidth * Actions.Length + Gap * (Actions.Length - 1);
        float left = (screen.X - rowWidth) / 2f;
        float top = screen.Y - ButtonHeight - BottomMargin;
        RectF[] buttons = new RectF[Actions.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = new RectF(left + i * (ButtonWidth + Gap), top, ButtonWidth, ButtonHeight);
        }

        return buttons;
    }

    private void LogLayout(Vector2 screen)
    {
        if (screen == _loggedScreen)
        {
            return;
        }

        _loggedScreen = screen;
        RectF[] buttons = Layout(screen);
        string centres = string.Join(' ', Actions.Select((name, i) => $"{name} {(int)buttons[i].Center.X},{(int)buttons[i].Center.Y}"));
        Console.WriteLine($"screen: {(int)screen.X}x{(int)screen.Y}");
        Console.WriteLine($"buttons: {centres}");
    }
}
