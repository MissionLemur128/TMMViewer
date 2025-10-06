using System.Numerics;
using TMMLibrary.Utils;

namespace TMMLibrary.TMM.DataTypes;

public struct BoundingBox : IEncode
{
    public Vector3 Min;
    public Vector3 Max;

    public BoundingBox()
    {
        Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    }

    public BoundingBox(IEnumerable<Vector3> points) : this()
    {
        foreach (var point in points)
        {
            Expand(point);
        }
    }

    public void Expand(Vector3 value)
    {
        Min = new Vector3(MathF.Min(Min.X, value.X), MathF.Min(Min.Y, value.Y), MathF.Min(Min.Z, value.Z));
        Max = new Vector3(MathF.Max(Max.X, value.X), MathF.Max(Max.Y, value.Y), MathF.Max(Max.Z, value.Z));
    }

    public static BoundingBox Decode(BinaryReader br)
    {
        return new BoundingBox
        {
            Min = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
            Max = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle())
        };
    }

    public void Encode(BinaryWriter bw)
    {
        bw.Write(Min.X);
        bw.Write(Min.Y);
        bw.Write(Min.Z);
        bw.Write(Max.X);
        bw.Write(Max.Y);
        bw.Write(Max.Z);
    }
}
