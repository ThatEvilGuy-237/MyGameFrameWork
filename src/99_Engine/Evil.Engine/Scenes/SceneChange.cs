namespace Evil.Engine.Scenes;

internal readonly record struct SceneChange(SceneChangeKind Kind, Scene? Scene);
