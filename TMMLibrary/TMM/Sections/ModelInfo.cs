using System.Diagnostics;
using System.Runtime.InteropServices;
using TMMLibrary.TMMData.DataTypes;
using TMMLibrary.Utils;

namespace TMMLibrary.TMM.Sections;

/// <summary>
/// Describes the model information of a .tmm.data file.
/// </summary>
public class ModelInfo 
{
    // Data Table
    public int AssignedMaterialCount { get; set; }
    public int MaterialCount { get; set; }
    public int ShaderCount { get; set; }
    public int BoneCount { get; set; }
    public int UnknownCount1 { get; set; }
    public int AttachPointCount { get; set; }
    public uint VertexCount { get; set; }
    public uint IndexCount { get; set; }
    public uint VertexOffset { get; set; } // Normally 0x00
    public uint IndexOffset { get; set; }
    public uint IndexOffsetCopy { get; set; } // Same as indexoffset, not sure why exists
    public uint IndexByteCount { get; set; }
    public uint BoneWeightsOffset { get; set; }
    public uint BoneWeightsByteCount { get; set; }
    public uint SubObjectsIdsOffset { get; set; }
    public uint SubObjectsIdsByteCount { get; set; }
    public uint Unknown2Offset { get; set; }
    public uint Unknown2Count { get; set; }
    public uint MaskDataOffset { get; set; }
    public uint MaskDataByteCount { get; set; }
    public ushort EndByte { get; set; } // Normally 256

    public ModelInfo() 
    { 
    }

    public static ModelInfo Decode(TmmHeader header, BinaryReader br)
    {
        return new ModelInfo(header, br);
    }

    private ModelInfo(TmmHeader header, BinaryReader br)
    {
        var myType = typeof(ModelInfo);

        AssignedMaterialCount = br.ReadInt32();
        MaterialCount = br.ReadInt32();
        ShaderCount = br.ReadInt32();
        BoneCount = br.ReadInt32();
        UnknownCount1 = br.ReadInt32();
        AttachPointCount = br.ReadInt32();
        VertexCount = br.ReadUInt32();
        IndexCount = br.ReadUInt32();
        //if (VertexCount % 2 == 0 && (header.FileFlags & (uint)FileFlags.HasOddVertexCount) > 0)
        //{
        //    throw new DecodeException(typeof(ModelInfo), $"IndexCount is not even when {nameof(TmmHeader.FileFlags)} is 34");
        //}

        VertexOffset = br.ReadUInt32();
        IndexOffset = br.ReadUInt32();
        IndexOffsetCopy = br.ReadUInt32(); // IndexOffset == IndexOffset2
        IndexByteCount = br.ReadUInt32(); // IndexByteCount == IndexCount * sizeof(short)
        if (IndexByteCount != IndexCount * 2)
        {
            throw new DecodeException(typeof(ModelInfo), "IndexByteCount != IndexCount * 2");
        }

        BoneWeightsOffset = br.ReadUInt32();
        BoneWeightsByteCount = br.ReadUInt32();

        SubObjectsIdsOffset = br.ReadUInt32();
        SubObjectsIdsByteCount = br.ReadUInt32();

        Unknown2Count = br.ReadUInt32();
        Unknown2Offset = br.ReadUInt32();

        MaskDataOffset = br.ReadUInt32();
        MaskDataByteCount = br.ReadUInt32();

        br.ReadZeroBytesOrThrow(myType, 4);

        // Unsure why the number of bytes changes, but seems consistently based on FileVersion
        if (header.FileVersion == 34)
            br.ReadBytes(3);
        else if (header.FileVersion == 35)
            br.ReadBytes(4);
        else
            throw new DecodeException(typeof(ModelInfo), "header.FileVersion != 34 or 35, don't know how to parse it.");


        EndByte = br.ReadUInt16();
        if (EndByte != 256)
            throw new DecodeException(typeof(ModelInfo), "modelInfo.EndBytes != 256");
    }

    public void Encode(TmmHeader header, BinaryWriter bw)
    {
        bw.Write(AssignedMaterialCount);
        bw.Write(MaterialCount);
        bw.Write(ShaderCount);
        bw.Write(BoneCount);
        bw.Write(UnknownCount1);
        bw.Write(AttachPointCount);
        bw.Write(VertexCount);

        Debug.Assert(IndexCount % 3 == 0);
        bw.Write(IndexCount);
        bw.Write(VertexOffset);
        bw.Write(IndexOffset);

        Debug.Assert(IndexOffset == IndexOffsetCopy);
        bw.Write(IndexOffsetCopy);

        Debug.Assert(IndexByteCount == IndexCount * sizeof(ushort));
        bw.Write(IndexByteCount);

        bw.Write(BoneWeightsOffset);
        Debug.Assert(BoneWeightsByteCount % VertexCount == 0);
        bw.Write(BoneWeightsByteCount);

        bw.Write(SubObjectsIdsOffset);
        bw.Write(SubObjectsIdsByteCount);
        bw.Write(Unknown2Count);
        bw.Write(Unknown2Offset);

        bw.Write(MaskDataOffset);
        Debug.Assert(MaskDataByteCount == VertexCount * sizeof(ushort));
        bw.Write(MaskDataByteCount);

        bw.WriteZerosBytes(4);

        if (header.FileVersion == 34)
            bw.WriteZerosBytes(3);
        else if (header.FileVersion == 35)
            bw.WriteZerosBytes(4);
        else
            throw new InvalidOperationException("Unsupported file version for encoding ModelInfo.");

        bw.Write(EndByte);
    }

    internal void Update(TmmModel tmmModel)
    {
        var tmmHeader = tmmModel.HeaderFile;    
        var tmmData = tmmModel.DataFile;

        // Update counts
        BoneCount = tmmHeader.Bones.Length;
        VertexCount = (uint)tmmData.Vertices.Length;
        IndexCount = (uint)tmmData.Indices.Length;
        IndexByteCount = IndexCount * sizeof(ushort);
        BoneWeightsByteCount = (uint)( BoneCount == 0 ? 0 : tmmData.BoneWeights.First().Stride * VertexCount);
        MaskDataByteCount = sizeof(ushort) * VertexCount;

        // Calculate offsets
        uint offset = 0;
        VertexOffset = offset;
        offset += VertexCount * Vertex.Stride;

        IndexOffset = offset;
        IndexOffsetCopy = offset;
        offset += IndexByteCount;

        if (BoneCount > 0)
        {
            BoneWeightsOffset = offset;
            offset += BoneWeightsByteCount;
        }
        else
        {
            BoneWeightsOffset = 0;
        }

        MaskDataOffset = offset;
    }
}
