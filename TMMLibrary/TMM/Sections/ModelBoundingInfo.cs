using TMMLibrary.TMM.DataTypes;
using TMMLibrary.Utils;

namespace TMMLibrary.TMM.Sections;

public class ModelBoundingInfo : IEncode
{
    public BoundingBox BoundingBoxes { get; set; }
    public BoundingBox LooseBoundingBoxes { get; set; }
    public float BoundingRadius { get; set; } // Guess?

    public static ModelBoundingInfo Decode(BinaryReader br)
    {
        return new ModelBoundingInfo
        {
            BoundingBoxes = BoundingBox.Decode(br),
            LooseBoundingBoxes = BoundingBox.Decode(br),
            BoundingRadius = br.ReadSingle(),
        };
    }

    public void Encode(BinaryWriter bw)
    {
        BoundingBoxes.Encode(bw);
        LooseBoundingBoxes.Encode(bw);
        bw.Write(BoundingRadius);
    }
}
