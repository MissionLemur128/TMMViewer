using System.Numerics;
using TMMLibrary.IOUtils;
using TMMLibrary.Utils;

namespace TMMLibrary.TMMData.DataTypes;

public struct Vertex : IEncode
{
    public Vector3 Origin;
    public Vector2 Uv;
    public Vector3 Normal;
    public Vector4 Tangent;

    public static Vertex Decode(BinaryReader br)
    {
        var r = new Vertex
        {
            Origin = new Vector3
            {
                X = (float)br.ReadHalf(),
                Y = (float)br.ReadHalf(),
                Z = (float)br.ReadHalf(),
            },
            Uv = new Vector2
            {
                X = (float)br.ReadHalf(),
                Y = (float)br.ReadHalf(),
            },
        };

        NormalEncoding.DecodeTangentSpace(br.ReadBytes(6), out r.Normal, out r.Tangent);
        return r;
    }

    public void Encode(BinaryWriter w)
    {
        w.Write((Half)Origin.X);
        w.Write((Half)Origin.Y);
        w.Write((Half)Origin.Z);
        w.Write((Half)Uv.X);
        w.Write((Half)Uv.Y);

        var encodedNormalData = NormalEncoding.EncodeDecodeTangentSpace(Normal, Tangent);
        Array.ForEach(encodedNormalData, w.Write);
    }

    public static uint Stride
    {
        get => 2 * 3 // position (half)
            + 2 * 2 // uv (half)
            + 6; // normal & tangents
    }
}
