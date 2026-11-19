namespace Evil.Engine.GodotTests.Characters;

internal static class CharacterTests
{
    [Test]
    public static void ACharacterLoadsAsItsPackageDescribesIt()
    {
        ContentLibrary content = new ContentLibrary(new PuppetPackage().Root);
        CharacterModel puppet = content.Character("puppet");
        Check.Equal("puppet", puppet.Name, "name");
        Check.Near(1f, puppet.Height, "two metres tall, shown at half size");
        Check.Equal("hat cap scarf", string.Join(' ', puppet.Clothing), "clothing");
        Check.Equal("dressed", string.Join(' ', puppet.Outfits), "outfits");
        Check.Equal("stand wave", string.Join(' ', puppet.Clips.Names), "clips");
        Check.Equal(0, puppet.Worn.Count, "bare at first");
        Check.True(puppet.Clips.Current is null, "still at first");
        puppet.Destroy();
        content.Close();
    }

    [Test]
    public static void ClothingGoesOnTheCharactersOwnRig()
    {
        ContentLibrary content = new ContentLibrary(new PuppetPackage().Root);
        using Stage stage = Stage.Open();
        CharacterModel puppet = stage.Root.Add(content.Character("puppet"));
        NodeSearch search = new NodeSearch(puppet);
        puppet.Wear("hat");
        Check.Equal("hat", string.Join(' ', puppet.Worn), "worn");
        Godot.MeshInstance3D hat = search.Mesh("hat") ?? throw new CheckFailed("the hat does not hang on the character's rig");
        Check.Equal("Head", hat.Skin.GetBindName(1).ToString(), "bound to the bone by its name");
        Check.Equal(1, search.Count<Godot.Skeleton3D>(), "the hat's own rig is gone");
        Check.Near(0.2f, search.Material("hat").GetShaderParameter("base_color").AsVector3().X, "painted in the colour its package gives");
        puppet.TakeOff("hat");
        Check.True(search.Mesh("hat") is null && puppet.Worn.Count == 0, "taken off");
        content.Close();
    }

    [Test]
    public static void ClothingHidesTheSkinItCovers()
    {
        ContentLibrary content = new ContentLibrary(new PuppetPackage().Root);
        using Stage stage = Stage.Open();
        CharacterModel puppet = stage.Root.Add(content.Character("puppet"));
        Check.Equal(4, puppet.SkinTriangles, "the whole skin");
        puppet.Wear("scarf");
        Check.Equal(2, puppet.SkinTriangles, "the scarf hides one patch");
        puppet.Wear("hat");
        Check.Equal(0, puppet.SkinTriangles, "the hat hides the other");
        puppet.TakeOff("scarf");
        Check.Equal(2, puppet.SkinTriangles, "its patch shows again");
        puppet.TakeOff("hat");
        Check.Equal(4, puppet.SkinTriangles, "bare again");
        content.Close();
    }

    [Test]
    public static void ClothingSetsTheShapesItNeeds()
    {
        ContentLibrary content = new ContentLibrary(new PuppetPackage().Root);
        using Stage stage = Stage.Open();
        CharacterModel puppet = stage.Root.Add(content.Character("puppet"));
        Godot.MeshInstance3D face = new NodeSearch(puppet).Mesh("face") ?? throw new CheckFailed("no face");
        int blink = face.FindBlendShapeByName("eyes-blink");
        puppet.Wear("hat");
        Check.Near(0.5f, face.GetBlendShapeValue(blink), "the hat presses the shape it names");
        puppet.TakeOff("hat");
        Check.Near(0f, face.GetBlendShapeValue(blink), "and lets go of it");
        content.Close();
    }

    [Test]
    public static void OnePieceToASlotAndAnOutfitAtOnce()
    {
        ContentLibrary content = new ContentLibrary(new PuppetPackage().Root);
        using Stage stage = Stage.Open();
        CharacterModel puppet = stage.Root.Add(content.Character("puppet"));
        NodeSearch search = new NodeSearch(puppet);
        puppet.WearOutfit("dressed");
        Check.Equal("hat scarf", string.Join(' ', puppet.Worn), "the outfit's pieces");
        puppet.Wear("cap");
        Check.Equal("cap scarf", string.Join(' ', puppet.Worn), "the cap takes the hat's slot");
        Check.True(!search.Mesh("hat")!.Visible, "a part that is only there to throw a shadow is not drawn");
        void WearMissing() => puppet.Wear("crown");
        Check.Throws<ArgumentException>(WearMissing, "hat, cap, scarf");
        void DressMissing() => puppet.WearOutfit("royal");
        Check.Throws<ArgumentException>(DressMissing, "dressed");
        void DropMissing() => puppet.TakeOff("hat");
        Check.Throws<ArgumentException>(DropMissing, "not wearing 'hat'");
        content.Close();
    }

    [Test]
    public static void ClipsPlayByNameAndLoopAsThePackageSays()
    {
        ContentLibrary content = new ContentLibrary(new PuppetPackage().Root);
        using Stage stage = Stage.Open();
        CharacterModel puppet = stage.Root.Add(content.Character("puppet"));
        Godot.AnimationPlayer player = new NodeSearch(puppet).First<Godot.AnimationPlayer>();
        puppet.Clips.Play("stand");
        Check.Equal("stand", puppet.Clips.Current, "playing");
        Check.True(puppet.Clips.Has("wave") && !puppet.Clips.Has("dance"), "what it has");
        Check.True(puppet.Clips.Loops("stand") && !puppet.Clips.Loops("wave"), "which clips loop");
        Check.Near(2f, puppet.Clips.Seconds("stand"), "how long");
        Check.Equal(Godot.Animation.LoopModeEnum.Linear, player.GetAnimation("stand").LoopMode, "Godot was told to loop it");
        puppet.Clips.Then("wave");
        Check.Equal("wave", string.Join(' ', player.GetQueue()), "waiting for its turn");
        puppet.Clips.Play("wave");
        Check.Equal(0, player.GetQueue().Count, "a new clip clears what was waiting");
        void PlayMissing() => puppet.Clips.Play("dance");
        Check.Throws<ArgumentException>(PlayMissing, "stand, wave");
        content.Close();
    }

    [Test]
    public static void AClipShowsTheFacesItNames()
    {
        ContentLibrary content = new ContentLibrary(new PuppetPackage().Root);
        using Stage stage = Stage.Open();
        CharacterModel puppet = stage.Root.Add(content.Character("puppet"));
        NodeSearch search = new NodeSearch(puppet);
        puppet.Clips.Play("stand");
        search.First<Godot.AnimationPlayer>().Seek(1.5, true);
        puppet.ShowFaceNow();
        Check.Equal(content.Texture("puppet/pictures/face-blink.png").Native.GetInstanceId(), Shown(search), "the blink picture from second 1 on");
        search.First<Godot.AnimationPlayer>().Seek(0.5, true);
        puppet.ShowFaceNow();
        Check.Equal(content.Texture("puppet/pictures/face-paint.png").Native.GetInstanceId(), Shown(search), "the plain picture before it");
        content.Close();
    }

    [Test]
    public static void TwoCharactersFromOnePackageAreTheirOwn()
    {
        ContentLibrary content = new ContentLibrary(new PuppetPackage().Root);
        using Stage stage = Stage.Open();
        CharacterModel first = stage.Root.Add(content.Character("puppet"));
        CharacterModel second = stage.Root.Add(content.Character("puppet"));
        first.WearOutfit("dressed");
        Check.Equal(0, first.SkinTriangles, "the dressed one");
        Check.Equal(4, second.SkinTriangles, "the other keeps its skin");
        Check.Equal(0, second.Worn.Count, "and wears nothing");
        first.Destroy();
        Check.True(first.IsDestroyed && !second.IsDestroyed, "one can go, the other stays");
        content.Close();
    }

    [Test]
    public static void AMissingOrBrokenPackageSaysWhatIsWrong()
    {
        PuppetPackage package = new PuppetPackage();
        ContentLibrary content = new ContentLibrary(package.Root);
        void LoadMissing() => content.Character("nobody");
        Check.Throws<FileNotFoundException>(LoadMissing, "nobody/character.json");
        File.WriteAllText(Path.Combine(package.Folder, "character.json"), "{ \"format\": 2 }");
        void LoadNewer() => content.Character("puppet");
        Check.Throws<InvalidDataException>(LoadNewer, "format 2");
        File.WriteAllText(Path.Combine(package.Folder, "character.json"), "{ \"format\": 1 }");
        void LoadEmpty() => content.Character("puppet");
        Check.Throws<InvalidDataException>(LoadEmpty, "has no 'outline'");
        content.Close();
    }

    [Test]
    public static void AClipThatIsNotAsLongAsItsDescriptionSaysIsRefused()
    {
        PuppetPackage package = new PuppetPackage();
        string told = Path.Combine(package.Folder, "character.json");
        File.WriteAllText(told, File.ReadAllText(told).Replace("\"wave\": {\"seconds\": 1.0", "\"wave\": {\"seconds\": 4.0"));
        ContentLibrary content = new ContentLibrary(package.Root);
        void Load() => content.Character("puppet");
        Check.Throws<InvalidDataException>(Load, "the clip 'wave' of");
        Check.Throws<InvalidDataException>(Load, "1.00 s long in its body file, its description says 4.00 s");
        content.Close();
    }

    [Test]
    public static void AClothingFileWithoutItsPartIsRefusedAndTheCharacterStaysAsItWas()
    {
        PuppetPackage package = new PuppetPackage();
        string told = Path.Combine(package.Folder, "character.json");
        string text = File.ReadAllText(told);
        int scarf = text.IndexOf("\"scarf\"", StringComparison.Ordinal);
        File.WriteAllText(told, text[..scarf] + text[scarf..].Replace("{\"name\": \"hat\"", "{\"name\": \"collar\""));
        ContentLibrary content = new ContentLibrary(package.Root);
        using Stage stage = Stage.Open();
        CharacterModel puppet = stage.Root.Add(content.Character("puppet"));
        puppet.Wear("hat");
        int drawn = puppet.SkinTriangles;
        void WearBroken() => puppet.Wear("scarf");
        Check.Throws<InvalidDataException>(WearBroken, "'collar'");
        Check.Equal("hat", string.Join(' ', puppet.Worn), "still wears what it wore");
        Check.Equal(drawn, puppet.SkinTriangles, "and shows the skin it showed");
        content.Close();
    }

    private static ulong Shown(NodeSearch search) => search.Material("face").GetShaderParameter("paint_a").As<Godot.Texture2D>().GetInstanceId();
}
