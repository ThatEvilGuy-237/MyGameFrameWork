namespace OrbitDemo;

internal sealed class Hud(Font titleFont)
{
    private const string Help = "Space pause   Q/E turn   wheel zoom   M music   F10 small   F11 full screen";

    public void Draw(Canvas canvas, string status, float cameraYaw)
    {
        DrawPanel(canvas, status);
        DrawCompass(canvas, cameraYaw);
    }

    private void DrawPanel(Canvas canvas, string status)
    {
        RectF panel = new RectF(16f, 16f, 480f, 112f);
        canvas.FillRect(panel, Color.Black.WithAlpha(0.45f));
        canvas.DrawRect(panel, Color.White.WithAlpha(0.25f));
        canvas.Font = titleFont;
        canvas.DrawText("Evil Engine orbit demo", new Vector2(30f, 24f), 24f, Color.White);
        canvas.Font = null;
        canvas.DrawText(status, new Vector2(30f, 60f), 16f, Color.FromHex("#FFC53D"));
        canvas.DrawText(Help, new Vector2(30f, 92f), 13f, Color.White.WithAlpha(0.7f));
    }

    private static void DrawCompass(Canvas canvas, float cameraYaw)
    {
        using TransformScope compass = canvas.Group();
        canvas.Translate(canvas.Size.X - 70f, 70f);
        canvas.FillCircle(Vector2.Zero, 46f, Color.Black.WithAlpha(0.45f));
        canvas.Rotate(cameraYaw);
        canvas.FillTriangle(new Vector2(0f, -32f), new Vector2(-9f, 0f), new Vector2(9f, 0f), Color.FromHex("#E5484D"));
        canvas.FillTriangle(new Vector2(0f, 32f), new Vector2(-9f, 0f), new Vector2(9f, 0f), Color.White.WithAlpha(0.8f));
        canvas.DrawText("N", new RectF(-10f, -52f, 20f, 16f), 13f, Color.White, TextAlign.Center);
    }
}
