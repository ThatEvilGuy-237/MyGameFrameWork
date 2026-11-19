using Evil.Engine.GodotTests.Content;

namespace Evil.Engine.GodotTests.Characters;

internal sealed class PuppetPackage
{
    private const string Rig = "Skeleton3D";

    public PuppetPackage()
    {
        Root = Path.Combine(Path.GetTempPath(), $"evil-engine-characters-{Guid.NewGuid():N}");
        Folder = Path.Combine(Root, "puppet");
        Directory.CreateDirectory(Path.Combine(Folder, "clothing"));
        Directory.CreateDirectory(Path.Combine(Folder, "pictures"));
        string told = Path.Combine(Godot.ProjectSettings.GlobalizePath("res://"), "Characters", "puppet", "character.json");
        File.Copy(told, Path.Combine(Folder, "character.json"));
        WriteBody(Path.Combine(Folder, "body.glb"));
        WriteHat(Path.Combine(Folder, "clothing", "hat.glb"));
        WritePicture(Path.Combine(Folder, "pictures", "face-paint.png"), new Godot.Color(1f, 0f, 0f));
        WritePicture(Path.Combine(Folder, "pictures", "face-blink.png"), new Godot.Color(0f, 0f, 1f));
    }

    public string Root { get; }

    public string Folder { get; }

    private static void WriteBody(string path)
    {
        Godot.Node3D root = new Godot.Node3D();
        root.Name = "puppet";
        Godot.Skeleton3D rig = Bones();
        root.AddChild(rig);
        rig.Owner = root;
        Hang(root, rig, "body", new PuppetMesh(2, 0).Plain());
        Hang(root, rig, "face", new PuppetMesh(1, 1).Shaped("eyes-blink"));
        Godot.AnimationPlayer player = new Godot.AnimationPlayer();
        player.Name = "AnimationPlayer";
        Godot.AnimationLibrary clips = new Godot.AnimationLibrary();
        clips.AddAnimation("stand", Nod(2f));
        clips.AddAnimation("wave", Nod(1f));
        player.AddAnimationLibrary(string.Empty, clips);
        root.AddChild(player);
        player.Owner = root;
        new ModelFile(path).Write(root);
    }

    private static void WriteHat(string path)
    {
        Godot.Node3D root = new Godot.Node3D();
        root.Name = "clothing";
        Godot.Skeleton3D rig = Bones();
        root.AddChild(rig);
        rig.Owner = root;
        Hang(root, rig, "hat", new PuppetMesh(1, 1).Plain());
        new ModelFile(path).Write(root);
    }

    private static Godot.Skeleton3D Bones()
    {
        Godot.Skeleton3D rig = new Godot.Skeleton3D();
        rig.Name = Rig;
        int hips = rig.AddBone("Hips");
        int head = rig.AddBone("Head");
        rig.SetBoneParent(head, hips);
        Godot.Transform3D raised = new Godot.Transform3D(Godot.Basis.Identity, new Godot.Vector3(0f, 1f, 0f));
        rig.SetBoneRest(head, raised);
        rig.SetBonePosePosition(head, raised.Origin);
        return rig;
    }

    private static void Hang(Godot.Node3D root, Godot.Skeleton3D rig, string name, Godot.ArrayMesh mesh)
    {
        Godot.MeshInstance3D part = new Godot.MeshInstance3D();
        part.Name = name;
        part.Mesh = mesh;
        Godot.Skin skin = new Godot.Skin();
        skin.AddBind(0, Godot.Transform3D.Identity);
        skin.AddBind(1, new Godot.Transform3D(Godot.Basis.Identity, new Godot.Vector3(0f, -1f, 0f)));
        part.Skin = skin;
        rig.AddChild(part);
        part.Owner = root;
        part.Skeleton = new Godot.NodePath("..");
    }

    private static Godot.Animation Nod(float seconds)
    {
        Godot.Animation clip = new Godot.Animation();
        clip.Length = seconds;
        int turn = clip.AddTrack(Godot.Animation.TrackType.Rotation3D);
        clip.TrackSetPath(turn, $"{Rig}:Head");
        clip.RotationTrackInsertKey(turn, 0.0, Godot.Quaternion.Identity);
        clip.RotationTrackInsertKey(turn, seconds / 2f, new Godot.Quaternion(Godot.Vector3.Right, 0.4f));
        clip.RotationTrackInsertKey(turn, seconds, Godot.Quaternion.Identity);
        int blink = clip.AddTrack(Godot.Animation.TrackType.BlendShape);
        clip.TrackSetPath(blink, $"{Rig}/face:eyes-blink");
        clip.BlendShapeTrackInsertKey(blink, 0.0, 0f);
        clip.BlendShapeTrackInsertKey(blink, seconds / 2f, 1f);
        clip.BlendShapeTrackInsertKey(blink, seconds, 0f);
        return clip;
    }

    private static void WritePicture(string path, Godot.Color colour)
    {
        Godot.Image image = Godot.Image.CreateEmpty(4, 4, false, Godot.Image.Format.Rgba8);
        image.Fill(colour);
        image.SavePng(path);
    }
}
