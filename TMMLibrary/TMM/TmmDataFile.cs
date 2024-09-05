using Assimp;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TMMLibrary.TMM;

public struct TmmVertex
{
    public Vector3 Origin;
    public Vector2 Uv;
    public Vector3 Tangent;
    //public ushort Unknown0;
    //public ushort Unknown1;
    //public ushort Unknown2;

    public static TmmVertex Decode(BinaryReader br)
    {
        var r = new TmmVertex
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
            Tangent = new Vector3
            {
                X = (float)br.ReadHalf(),
                Y = (float)br.ReadHalf(),
                Z = (float)br.ReadHalf(),
            }
        };
        //br.ReadBytes(3);
        return r;
    }

    public void Encode(BinaryWriter w)
    {
        w.Write((Half)Origin.X);
        w.Write((Half)Origin.Y);
        w.Write((Half)Origin.Z);
        w.Write((Half)Uv.X);
        w.Write((Half)Uv.Y);
        w.Write((Half)Tangent.X);
        w.Write((Half)Tangent.Y);
        w.Write((Half)Tangent.Z);
    }

    public void WriteObj(TextWriter w)
    {
        var uv2 = Uv;
        uv2.Y = 1f - uv2.Y;
        w.WriteLine($"v {Origin.X:.9f} {Origin.Y:.9f} {Origin.Z:.9f}");
        w.WriteLine($"vt {uv2.X:.9f} {uv2.Y:.9f}");
    }
}

public struct TmmBoneWeights
{
    public byte[] Weights { get; set; }
    public byte[] BoneIndices { get; set; }

    public static TmmBoneWeights Decode(BinaryReader br, int perVertexWeightCount)
    {
        return new TmmBoneWeights
        {
            Weights = br.ReadBytes(perVertexWeightCount),
            BoneIndices = br.ReadBytes(perVertexWeightCount)
        };
    }

    public void Encode(BinaryWriter w)
    {
        w.Write(Weights);
        w.Write(BoneIndices);
    }
}

public class TmmDataFile
{
    public TmmVertex[] Vertices { get; set; }
    public ushort[] Indices { get; set; }
    public TmmBoneWeights[] BoneWeights { get; set; }
    public byte[] Mask { get; set; }

    public static TmmDataFile Decode(ModelInfo modelInfo, string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        return Decode(modelInfo, br);
    }
    
    public static TmmDataFile Decode(ModelInfo modelInfo, BinaryReader br)
    {
        // Vertices
        var vertices = new TmmVertex[modelInfo.VertexCount];
        br.BaseStream.Seek(modelInfo.VertexOffset, SeekOrigin.Begin);
        for (var i = 0; i < modelInfo.VertexCount; ++i)
        {
            vertices[i] = TmmVertex.Decode(br);
        }

        // Indices
        br.BaseStream.Seek(modelInfo.IndexOffset, SeekOrigin.Begin);
        var indices = br.ReadUint16Array((int)modelInfo.IndexCount);

        // Bone Weights: 2 = 1 byte for weight, 1 byte for bone index
        br.BaseStream.Seek(modelInfo.BoneWeightsOffset, SeekOrigin.Begin);
        var boneWeights = new TmmBoneWeights[modelInfo.VertexCount];
        var maxBonePerVertexCount = (int)(modelInfo.BoneWeightsByteCount / modelInfo.VertexCount / 2);
        for (var i = 0; i < modelInfo.VertexCount; ++i)
        {
            boneWeights[i] = TmmBoneWeights.Decode(br, maxBonePerVertexCount);
        }
        
        // Mask data
        br.BaseStream.Seek(modelInfo.MaskDataOffset, SeekOrigin.Begin);
        var mask = br.ReadBytes((int)modelInfo.MaskDataByteCount);

        return new TmmDataFile
        {
            Vertices = vertices,
            Indices = indices,
            BoneWeights = boneWeights,
            Mask = mask
        };
    }

    public void Encode(BinaryWriter w)
    {
        Array.ForEach(Vertices, r => r.Encode(w));
        Array.ForEach(Indices, w.Write);
        Array.ForEach(BoneWeights, r => r.Encode(w));
        Array.ForEach(Mask, w.Write);
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