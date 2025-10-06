using System.Numerics;
using TMMLibrary.Utils;

namespace TMMLibrary.TMM.DataTypes;

public class AttachPoint : IEncode
{
    public int BoneParent;
    public string Name1 = "";
    public Matrix4x4 LocalTransform;
    public Matrix4x4 LocalTransformCopy;

    // Second name, usually the same as the initial name when present but not always present.
    public string Name2 = "";

    public static AttachPoint Decode(BinaryReader br)
    {
        var myType = typeof(AttachPoint);
        br.ReadZeroBytesOrThrow(myType, 4);

        var apoint = new AttachPoint();
        apoint.BoneParent = br.ReadInt32();
        apoint.Name1 = br.ReadTmString();
        apoint.LocalTransform = br.ReadMatrix4x3();
        apoint.LocalTransformCopy = br.ReadMatrix4x3();

        br.ReadZeroBytesOrThrow(myType, sizeof(int) * 2);

        // Next value is always a TmString, but it may be zero-length.
        apoint.Name2 = br.ReadTmString();

        var tmp1 = br.ReadInt32();
        DecodeException.ExpectEqual(myType, br.BaseStream.Position - sizeof(int), tmp1, -1);

        br.ReadZeroBytesOrThrow(myType, sizeof(int) * 3);

        return apoint;
    }

    public void Encode(BinaryWriter bw)
    {
        bw.WriteZerosBytes(sizeof(int));

        bw.Write(BoneParent);
        bw.WriteTmString(Name1);
        bw.WriteMatrix4x3(LocalTransform);
        bw.WriteMatrix4x3(LocalTransformCopy);

        bw.WriteZerosBytes(sizeof(int) * 2); 
        bw.WriteTmString(Name2);

        bw.Write(-1);
        bw.WriteZerosBytes(sizeof(int) * 3);
    }
}
