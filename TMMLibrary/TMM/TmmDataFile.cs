using System.Numerics;
using System.Runtime.InteropServices;

namespace TMMLibrary.TMM;

public struct TmmVertex
{
    public Vector3 Origin;
    public Vector2 Uv;
    public Vector3 Normal;
    //public ushort Unknown0;
    //public ushort Unknown1;
    //public ushort Unknown2;

    public static TmmVertex Decode(BinaryReader br)
    {
        return new TmmVertex
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
            Normal = new Vector3
            {
                X = (float)br.ReadHalf(),
                Y = (float)br.ReadHalf(),
                Z = (float)br.ReadHalf(),
            },
            //Unknown0 = br.ReadUInt16(),
            //Unknown1 = br.ReadUInt16(),
            //Unknown2 = br.ReadUInt16(),
        };
    }

    public void Encode(BinaryWriter w)
    {
        w.Write((Half)Origin.X);
        w.Write((Half)Origin.Y);
        w.Write((Half)Origin.Z);
        w.Write((Half)Uv.X);
        w.Write((Half)Uv.Y);
        w.Write((Half)Normal.X);
        w.Write((Half)Normal.Y);
        w.Write((Half)Normal.Z);

        //w.Write(Unknown0);
        //w.Write(Unknown1);
        //w.Write(Unknown2);
    }

    public void WriteObj(TextWriter w)
    {
        var uv2 = Uv;
        uv2.Y = 1f - uv2.Y;
        w.WriteLine($"v {Origin.X:.9f} {Origin.Y:.9f} {Origin.Z:.9f}");
        w.WriteLine($"vt {uv2.X:.9f} {uv2.Y:.9f}");
    }
}

public class TmmDataFile
{
    public TmmVertex[] Vertices { get; set; }
    public ushort[] Indices { get; set; }
    public byte[] Unknown1 { get; set; }
    public byte[] Unknown2 { get; set; }

    public static TmmDataFile Decode(ModelInfo modelInfo, string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        return Decode(modelInfo, br);
    }
    
    public static TmmDataFile Decode(ModelInfo modelInfo, BinaryReader br)
    {
        var vertices = new TmmVertex[modelInfo.VertexCount];
        br.BaseStream.Seek(modelInfo.VertexOffset, SeekOrigin.Begin);
        for (var i = 0; i < modelInfo.VertexCount; ++i)
        {
            vertices[i] = TmmVertex.Decode(br);
        }

        br.BaseStream.Seek(modelInfo.IndexOffset, SeekOrigin.Begin);
        var indices = br.ReadUint16Array((int)modelInfo.IndexCount);

        br.BaseStream.Seek(modelInfo.UnknownData1Offset, SeekOrigin.Begin);
        var unknown1 = br.ReadBytes((int)modelInfo.UnknownData1Count);
        
        br.BaseStream.Seek(modelInfo.UnknownData2Offset, SeekOrigin.Begin);
        var unknown2 = br.ReadBytes((int)modelInfo.UnknownData2Count);

        return new TmmDataFile
        {
            Vertices = vertices,
            Indices = indices,
            Unknown1 = unknown1,
            Unknown2 = unknown2
        };
    }

    public void Encode(BinaryWriter w)
    {
        for (var i = 0; i < Vertices.Length; ++i)
        {
            Vertices[i].Encode(w);
        }

        for (var i = 0; i < Indices.Length; ++i)
        {
            w.Write(Indices[i]);
        }
        
        w.Write(Unknown1);
        w.Write(Unknown2);
    }

    public void WriteObj(TextWriter w)
    {
        foreach (var vertex in Vertices)
        {
            vertex.WriteObj(w);
        }

        for (var i = 0; i < Indices.Length; i += 3)
        {
            var face = new ArraySegment<ushort>(Indices, i, 3);
            // ReSharper disable HeapView.BoxingAllocation
            w.WriteLine("f {0}/{0} {1}/{1} {2}/{2}", face[0], face[1], face[2]);
            // ReSharper enable HeapView.BoxingAllocation
        }
    }
}