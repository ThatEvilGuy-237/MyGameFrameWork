using Godot;

namespace Evil.Engine.Characters;

internal sealed class SkinBinder
{
    private static readonly NodePath Parent = new NodePath("..");

    private readonly Skeleton3D _rig;

    public SkinBinder(Skeleton3D rig) => _rig = rig;

    public void Bind(MeshInstance3D mesh, Skeleton3D from)
    {
        Skin own = mesh.Skin ?? throw new InvalidDataException($"Character: the clothing part '{mesh.Name}' is not skinned to a rig.");
        Skin skin = new Skin();
        for (int bind = 0; bind < own.GetBindCount(); bind++)
        {
            string bone = own.GetBindName(bind).ToString();
            if (bone.Length == 0)
            {
                bone = from.GetBoneName(own.GetBindBone(bind));
            }

            if (_rig.FindBone(bone) < 0)
            {
                throw new InvalidDataException($"Character: the clothing part '{mesh.Name}' hangs on the bone '{bone}', which this character's rig does not have.");
            }

            skin.AddNamedBind(bone, own.GetBindPose(bind));
        }

        mesh.GetParent().RemoveChild(mesh);
        mesh.Owner = null;
        _rig.AddChild(mesh);
        mesh.Skin = skin;
        mesh.Skeleton = Parent;
    }
}
