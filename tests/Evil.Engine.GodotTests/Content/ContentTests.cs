using Evil.Engine.Input;

namespace Evil.Engine.GodotTests.Content;

internal static class ContentTests
{
    [Test]
    public static void ModelsLoadWithTheirParts()
    {
        string folder = TempFolder();
        WriteRobot(Path.Combine(folder, "robot.glb"));
        Model model = new ContentLibrary(folder).Model("robot.glb");
        Check.Equal("robot", model.Name, "named after the file");
        Check.Equal("Body Hand", PartNames(model), "both meshes are parts");
        Check.True(model.Find("Arm") is { } arm && arm is not ModelPart, "the empty node is a group");
        Check.Equal("Arm", model.Part("Hand")?.Parent?.Name, "the hand hangs on the arm");

        using Stage stage = Stage.Open();
        stage.Root.Add(model);
        model.Position = new Vector3(0f, 0f, -1f);
        Check.Near(new Vector3(3f, 0f, -1f), model.Part("Hand")!.WorldPosition, "parts follow the model");
        model.Destroy();
        Check.True(model.Parts.Count == 0 || model.IsDestroyed, "destroyed");
    }

    [Test]
    public static void EachModelCallGivesItsOwnCopyOfOneLoadedFile()
    {
        string folder = TempFolder();
        WriteRobot(Path.Combine(folder, "robot.glb"));
        ContentLibrary content = new ContentLibrary(folder);
        Model first = content.Model("robot.glb");
        File.Delete(Path.Combine(folder, "robot.glb"));
        Model second = content.Model("robot.glb");
        Check.True(!ReferenceEquals(first, second) && !ReferenceEquals(first.Part("Hand"), second.Part("Hand")), "two separate models");
        Check.Equal(2, second.Parts.Count, "the second copy has its parts too");
        first.Part("Hand")!.Position = new Vector3(9f, 0f, 0f);
        Check.Near(new Vector3(1f, 0f, 0f), second.Part("Hand")!.Position, "moving a part of one copy leaves the other alone");
        first.Destroy();
        second.Destroy();
        content.Close();
    }

    [Test]
    public static void ModelPartsCanBePickedAndPainted()
    {
        string folder = TempFolder();
        WriteRobot(Path.Combine(folder, "robot.glb"));
        Model model = new ContentLibrary(folder).Model("robot.glb");
        using Stage stage = Stage.Open();
        stage.Root.Add(model);
        Camera camera = stage.AddCamera(new Vector3(0f, 0f, 10f));
        Vector2 middle = Stage.ScreenMiddle;
        Hit? hit = Picker.Pick(camera, stage.Root, middle);
        Check.Equal("Body", hit?.Target.Name, "the body is under the middle of the screen");
        Check.True(hit!.Target.Ancestor<Model>() == model, "and belongs to the model");

        Material red = Material.Matte(Color.Red);
        model.Part("Body")!.Material = red;
        Check.True(ReferenceEquals(red.Native, model.Part("Body")!.Instance.MaterialOverride), "painted");
        model.Paint(null);
        Check.True(model.Part("Body")!.Instance.MaterialOverride is null, "back to its own material");
    }

    [Test]
    public static void TexturesLoadOnceAndGoOnMaterials()
    {
        string folder = TempFolder();
        Godot.Image image = Godot.Image.CreateEmpty(4, 2, false, Godot.Image.Format.Rgba8);
        image.Fill(new Godot.Color(1f, 0f, 0f));
        image.SavePng(Path.Combine(folder, "red.png"));
        ContentLibrary content = new ContentLibrary(folder);
        Texture texture = content.Texture("red.png");
        Check.Equal(4, texture.Width, "width");
        Check.Equal(2, texture.Height, "height");
        Check.True(ReferenceEquals(texture, content.Texture("red.png")), "loaded once");

        Material material = new Material();
        material.Texture = texture;
        material.TextureScale = new Vector2(8f, 4f);
        Check.True(ReferenceEquals(texture.Native, material.Native.AlbedoTexture), "on the material");
        Check.Near(new Vector2(8f, 4f), material.TextureScale, "tiled");
        material.Texture = null;
        Check.True(material.Native.AlbedoTexture is null, "and off again");
    }

    [Test]
    public static void FontsLoadAndChangeTheText()
    {
        string project = Godot.ProjectSettings.GlobalizePath("res://");
        string lato = Path.GetFullPath(Path.Combine(project, "..", "..", "samples", "OrbitDemo", "content", "fonts", "Lato-Bold.ttf"));
        Font font = new ContentLibrary(TempFolder()).Font(lato);
        Godot.Node2D node = new Godot.Node2D();
        GodotCanvasTarget target = new GodotCanvasTarget(node);
        Vector2 plain = target.Measure("Evil Engine orbit demo", 24, null);
        Vector2 styled = target.Measure("Evil Engine orbit demo", 24, font);
        node.Free();
        Check.True(styled.X > 0f && Math.Abs(styled.X - plain.X) > 1f, $"Lato measures differently from the built-in font: {styled} vs {plain}");
    }

    [Test]
    public static void MissingContentSaysWhereItLooked()
    {
        string folder = TempFolder();
        ContentLibrary content = new ContentLibrary(folder);
        void LoadMissingTexture() => content.Texture("nope.png");
        Check.Throws<FileNotFoundException>(LoadMissingTexture, "'nope.png' was not found");
        void LoadMissingModel() => content.Model("models/nope.glb");
        Check.Throws<FileNotFoundException>(LoadMissingModel, Path.Combine(folder, "models", "nope.glb"));
    }

    [Test]
    public static void TheBuilderChecksTheContentFolder()
    {
        Check.Throws<InvalidOperationException>(
            () => GameBuilder.Create("test", 320, 240).Content("/no/such/folder").Start(new EmptyScene()).Build(),
            "GameBuilder.Content");
        string folder = TempFolder();
        Check.Equal(folder, GameBuilder.Create("test", 320, 240).Content(folder).Start(new EmptyScene()).Build().Options.ContentRoot, "a real folder");
        Check.Equal("res://", GameBuilder.Create("test", 320, 240).Start(new EmptyScene()).Build().Options.ContentRoot, "the game project by default");
    }

    private static string PartNames(Model model)
    {
        List<string> names = new List<string>();
        foreach (ModelPart part in model.Parts)
        {
            names.Add(part.Name);
        }

        names.Sort(StringComparer.Ordinal);
        return string.Join(' ', names);
    }

    private static string TempFolder()
    {
        string folder = Path.Combine(Path.GetTempPath(), $"evil-engine-content-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static void WriteRobot(string path)
    {
        Godot.Node3D root = new Godot.Node3D();
        root.Name = "robot";
        Godot.MeshInstance3D body = new Godot.MeshInstance3D();
        body.Name = "Body";
        body.Mesh = new Godot.BoxMesh();
        Godot.Node3D arm = new Godot.Node3D();
        arm.Name = "Arm";
        arm.Position = new Godot.Vector3(2f, 0f, 0f);
        Godot.MeshInstance3D hand = new Godot.MeshInstance3D();
        hand.Name = "Hand";
        hand.Mesh = new Godot.SphereMesh();
        hand.Position = new Godot.Vector3(1f, 0f, 0f);
        root.AddChild(body);
        root.AddChild(arm);
        arm.AddChild(hand);
        body.Owner = root;
        arm.Owner = root;
        hand.Owner = root;
        Godot.GltfDocument document = new Godot.GltfDocument();
        Godot.GltfState state = new Godot.GltfState();
        Godot.Error appended = document.AppendFromScene(root, state);
        Godot.Error written = document.WriteToFilesystem(state, path);
        root.Free();
        if (appended != Godot.Error.Ok || written != Godot.Error.Ok)
        {
            throw new CheckFailed($"could not write the test model: {appended}, {written}");
        }
    }
}
