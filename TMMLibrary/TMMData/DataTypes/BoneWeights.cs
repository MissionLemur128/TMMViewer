using TMMLibrary.Utils;

namespace TMMLibrary.TMMData.DataTypes;

public struct BoneWeights : IEncode
{
    public readonly int WeightsCount => Weights.Length;
    public byte[] Weights { get; set; }
    public byte[] BoneIndices { get; set; }
    public readonly int Stride => sizeof(byte) * (Weights.Length + BoneIndices.Length);

    public static BoneWeights Decode(BinaryReader br, int perVertexWeightCount)
    {
        return new BoneWeights
        {
            Weights = br.ReadBytes(perVertexWeightCount),
            BoneIndices = br.ReadBytes(perVertexWeightCount)
        };
    }

    public void Encode(BinaryWriter w)
    {
        Array.ForEach(Weights, w.Write);
        Array.ForEach(BoneIndices, w.Write);
    }
}
