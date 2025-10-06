using System.Numerics;
using TMMLibrary.Utils;

namespace TMMLibrary.TMM.DataTypes;

public class Bone : IEncode
{
    public string Name = "";
    public int BoneParentIndex;
    public float[] Unknown = []; // size = 4
    public Matrix4x4 LocalTransform = Matrix4x4.Identity;
    public Matrix4x4 GlobalTransform = Matrix4x4.Identity;
    public Matrix4x4 InverseGlobalTransform = Matrix4x4.Identity;

    public static Bone Decode(BinaryReader br)
    {
        return new Bone
        {
            Name = br.ReadTmString(),
            BoneParentIndex = br.ReadInt32(),
            Unknown = br.ReadFloat32Array(4),
            LocalTransform = br.ReadMatrix4x4(),
            GlobalTransform = br.ReadMatrix4x4(),
            InverseGlobalTransform = br.ReadMatrix4x4(),
        };
    }

    public void Encode(BinaryWriter bw)
    {
        bw.WriteTmString(Name);
        bw.Write(BoneParentIndex);
        Array.ForEach(Unknown, bw.Write);
        bw.Write(LocalTransform);
        bw.Write(GlobalTransform);
        bw.Write(InverseGlobalTransform);
    }
}
