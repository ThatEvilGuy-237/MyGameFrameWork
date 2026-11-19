using Godot;

namespace Evil.Engine.Characters;

internal interface ICharacterFiles
{
    string Source { get; }

    Node3D Scene(string file);

    Texture2D Picture(string file);
}
