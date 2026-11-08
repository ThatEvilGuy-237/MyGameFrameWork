using Evil.Engine.Input;

namespace Evil.Engine.GodotTests;

internal static class ContentTests
{
    [Test]
    public static void ModelsLoadWithTheirParts()
    {
        var folder = TempFolder();
        WriteRobot(Path.Combine(folder, "robot.glb"));
        var model = new ContentLibrary(folder).Model("robot.glb");
        Check.Equal("robot", model.Name, "named after the file");
        Check.Equal("Body Hand", string.Join(' ', model.Parts.Select(part => part.Name).Order()), "both meshes are parts");
        Check.True(model.Find("Arm") is { } arm && arm is not ModelPart, "the empty node is a group");
        Check.Equal("Arm", model.Part("Hand")?.Parent?.Name, "the hand hangs on the arm");

        using var stage = Stage.Open();
        stage.Root.Add(model);
        model.Position = new Vector3(0f, 0f, -1f);
        Check.Near(new Vector3(3f, 0f, -1f), model.Part("Hand")!.WorldPosition, "parts follow the model");
        model.Destroy();
        Check.True(model.Parts.Count == 0 || model.IsDestroyed, "destroyed");
    }

    [Test]
    public static void ModelPartsCanBePickedAndPainted()
    {
        var folder = TempFolder();
        WriteRobot(Path.Combine(folder, "robot.glb"));
        var model = new ContentLibrary(folder).Model("robot.glb");
        using var stage = Stage.Open();
        stage.Root.Add(model);
        var camera = stage.Root.Add(new Camera { Position = new Vector3(0f, 0f, 10f) });
        camera.LookAt(Vector3.Zero);
        camera.MakeCurrent();
        var middle = Stage.Host!.GetViewport().GetVisibleRect().Size.ToEvil() / 2f;
        var hit = Picker.Pick(camera, stage.Root, middle);
        Check.Equal("Body", hit?.Target.Name, "the body is under the middle of the screen");
        Check.True(hit!.Target.Ancestor<Model>() == model, "and belongs to the model");

        var red = Material.Matte(Color.Red);
        model.Part("Body")!.Material = red;
        Check.True(ReferenceEquals(red.Native, model.Part("Body")!.Instance.MaterialOverride), "painted");
        model.Paint(null);
        Check.True(model.Part("Body")!.Instance.MaterialOverride is null, "back to its own material");
    }

    [Test]
    public static void TexturesLoadOnceAndGoOnMaterials()
    {
        var folder = TempFolder();
        var image = Godot.Image.CreateEmpty(4, 2, false, Godot.Image.Format.Rgba8);
        image.Fill(new Godot.Color(1f, 0f, 0f));
        image.SavePng(Path.Combine(folder, "red.png"));
        var content = new ContentLibrary(folder);
        var texture = content.Texture("red.png");
        Check.Equal(4, texture.Width, "width");
        Check.Equal(2, texture.Height, "height");
        Check.True(ReferenceEquals(texture, content.Texture("red.png")), "loaded once");

        var material = new Material { Texture = texture, TextureScale = new Vector2(8f, 4f) };
        Check.True(ReferenceEquals(texture.Native, material.Native.AlbedoTexture), "on the material");
        Check.Near(new Vector2(8f, 4f), material.TextureScale, "tiled");
        material.Texture = null;
        Check.True(material.Native.AlbedoTexture is null, "and off again");
    }

    [Test]
    public static void FontsLoadAndChangeTheText()
    {
        var project = Godot.ProjectSettings.GlobalizePath("res://");
        var lato = Path.GetFullPath(Path.Combine(project, "..", "..", "samples", "OrbitDemo", "content", "fonts", "Lato-Bold.ttf"));
        var font = new ContentLibrary(TempFolder()).Font(lato);
        var node = new Godot.Node2D();
        var target = new GodotCanvasTarget(node);
        var plain = target.Measure("Evil Engine orbit demo", 24, null);
        var styled = target.Measure("Evil Engine orbit demo", 24, font);
        node.Free();
        Check.True(styled.X > 0f && Math.Abs(styled.X - plain.X) > 1f, $"Lato measures differently from the built-in font: {styled} vs {plain}");
    }

    [Test]
    public static void MissingContentSaysWhereItLooked()
    {
        var folder = TempFolder();
        var content = new ContentLibrary(folder);
        Check.Throws<FileNotFoundException>(() => content.Texture("nope.png"), "'nope.png' was not found");
        Check.Throws<FileNotFoundException>(() => content.Model("models/nope.glb"), Path.Combine(folder, "models", "nope.glb"));
    }

    [Test]
    public static void TheBuilderChecksTheContentFolder()
    {
        Check.Throws<InvalidOperationException>(
            () => GameBuilder.Create("test", 320, 240).Content("/no/such/folder").Start(new EmptyScene()).Build(),
            "GameBuilder.Content");
        var folder = TempFolder();
        Check.Equal(folder, GameBuilder.Create("test", 320, 240).Content(folder).Start(new EmptyScene()).Build().Options.ContentRoot, "a real folder");
        Check.Equal("res://", GameBuilder.Create("test", 320, 240).Start(new EmptyScene()).Build().Options.ContentRoot, "the game project by default");
    }

    private static string TempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"evil-engine-content-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static void WriteRobot(string path)
    {
        var root = new Godot.Node3D { Name = "robot" };
        var body = new Godot.MeshInstance3D { Name = "Body", Mesh = new Godot.BoxMesh() };
        var arm = new Godot.Node3D { Name = "Arm", Position = new Godot.Vector3(2f, 0f, 0f) };
        var hand = new Godot.MeshInstance3D { Name = "Hand", Mesh = new Godot.SphereMesh(), Position = new Godot.Vector3(1f, 0f, 0f) };
        root.AddChild(body);
        root.AddChild(arm);
        arm.AddChild(hand);
        body.Owner = root;
        arm.Owner = root;
        hand.Owner = root;
        var document = new Godot.GltfDocument();
        var state = new Godot.GltfState();
        var appended = document.AppendFromScene(root, state);
        var written = document.WriteToFilesystem(state, path);
        root.Free();
        if (appended != Godot.Error.Ok || written != Godot.Error.Ok)
        {
            throw new CheckFailed($"could not write the test model: {appended}, {written}");
        }
    }

    private sealed class EmptyScene : Scene;
}
