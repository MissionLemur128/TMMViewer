using TMMLibrary.TMM.Sections;
using TMMLibrary.TMMData.DataTypes;
using TMMLibrary.Utils;

namespace TMMLibrary.TMMData;

public class TmmDataFile
{
    public Vertex[] Vertices { get; set; } = [];
    public ushort[] Indices { get; set; } = [];
    public BoneWeights[] BoneWeights { get; set; } = [];
    public SubObject[] SubObjectIds { get; set; } = [];
    public byte[] HeightGradient { get; set; } = [];

    public static TmmDataFile Decode(ModelInfo modelInfo, string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        return Decode(modelInfo, br);
    }
    
    public static TmmDataFile Decode(ModelInfo modelInfo, BinaryReader br)
    {
        // Vertices
        br.BaseStream.Seek(modelInfo.VertexOffset, SeekOrigin.Begin);
        var vertices = br.DecodeArray((int)modelInfo.VertexCount, Vertex.Decode);

        // Indices
        br.BaseStream.Seek(modelInfo.IndexOffset, SeekOrigin.Begin);
        var indices = br.ReadUint16Array((int)modelInfo.IndexCount);
        
        // Bone Weights
        var boneWeights = new BoneWeights[modelInfo.VertexCount];
        if (modelInfo.VertexCount > 0)
        {
            br.BaseStream.Seek(modelInfo.BoneWeightsOffset, SeekOrigin.Begin);
            var maxBonePerVertexCount = (int)(modelInfo.BoneWeightsByteCount / modelInfo.VertexCount / 2); // 2 = 1 byte for weight, 1 byte for bone index
            for (var i = 0; i < modelInfo.VertexCount; ++i)
            {
                boneWeights[i] = DataTypes.BoneWeights.Decode(br, maxBonePerVertexCount);
            }
        }

        // SubObjects Ids
        var subObjectsCount = (int)(modelInfo.SubObjectsIdsByteCount / SubObject.Stride);
        var subObjectsIds = new SubObject[subObjectsCount];
        if (subObjectsCount > 0)
        {
            br.BaseStream.Seek(modelInfo.SubObjectsIdsOffset, SeekOrigin.Begin);
            subObjectsIds = br.DecodeArray(subObjectsCount, SubObject.Decode);
        }

        // Mask data
        br.BaseStream.Seek(modelInfo.MaskDataOffset, SeekOrigin.Begin);
        var heightGradient = br.ReadBytes((int)modelInfo.MaskDataByteCount);

        return new TmmDataFile
        {
            Vertices = vertices,
            Indices = indices,
            BoneWeights = boneWeights,
            SubObjectIds = subObjectsIds,
            HeightGradient = heightGradient
        };
    }

    public void Encode(BinaryWriter w)
    {
        w.EncodeArray(Vertices);
        Array.ForEach(Indices, w.Write);
        w.EncodeArray(BoneWeights);
        w.EncodeArray(SubObjectIds);
        Array.ForEach(HeightGradient, w.Write);
    }
}