using System.Numerics;
using TMMLibrary.TMM.DataTypes;
using TMMLibrary.TMM.Sections;
using TMMLibrary.Utils;

namespace TMMLibrary.TMM;

public class TmmFile
{
    public TmmHeader Header { get; set; }
    public ModelHeader[] ModelHeaders { get; set; }
    public ModelBoundingInfo BoundingInfo { get; set; }
    public ModelInfo ModelInfo { get; set; }
    public Matrix4x4 Transform { get; set; }
    public AttachPoint[] AttachPoints { get; set; }
    public MaterialVertexGroup[] MaterialVertexGroups { get; set; }
    public string[] Materials { get; set; }
    public string[] Shaders { get; set; }
    public Bone[] Bones { get; set; }
    public SubObject[] SubObjects { get; set; }
    public byte[] RemainingData { get; set; }

    public static TmmFile Decode(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        return new TmmFile(br);
    }

    public static TmmFile Decode(BinaryReader br)
    {
        return new TmmFile(br);
    }

    private TmmFile(BinaryReader br)
    {
        var type = typeof(TmmFile);
        Header = TmmHeader.Decode(br);

        var modelCount = br.ReadInt32();
        ModelHeaders = br.DecodeArray(modelCount, ModelHeader.Decode);

        BoundingInfo = ModelBoundingInfo.Decode(br);
        ModelInfo = ModelInfo.Decode(Header, br);
        Transform = br.ReadMatrix4x3();

        AttachPoints = br.DecodeArray(ModelInfo.AttachPointCount, AttachPoint.Decode);
        MaterialVertexGroups = br.DecodeArray(ModelInfo.AssignedMaterialCount, MaterialVertexGroup.Decode);
        Materials = br.DecodeArray(ModelInfo.MaterialCount, br => br.ReadTmString());
        Shaders = br.DecodeArray(ModelInfo.ShaderCount, br => br.ReadTmString());
        Bones = br.DecodeArray(ModelInfo.BoneCount, Bone.Decode);

        if (ModelInfo.SubObjectsIdsByteCount > 0)
        {
            br.ReadUInt16();
            br.ReadZeroBytesOrThrow(type, 4);
            var subObjectCount = br.ReadUInt32();
            SubObjects = br.DecodeArray((int)subObjectCount, SubObject.Decode);
        }
        else
        {
            SubObjects = [];
        }

        RemainingData = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
    }

    public void Encode(BinaryWriter bw)
    {
        Header.Encode(bw);

        bw.Write(ModelHeaders.Length);
        bw.EncodeArray(ModelHeaders);

        BoundingInfo.Encode(bw);
        ModelInfo.Encode(Header, bw);
        bw.WriteMatrix4x3(Transform);

        bw.EncodeArray(AttachPoints);
        bw.EncodeArray(MaterialVertexGroups);
        Array.ForEach(Materials, bw.WriteTmString);
        Array.ForEach(Shaders, bw.WriteTmString);
        bw.EncodeArray(Bones);

        if (SubObjects.Length > 0)
        {
            bw.Write((ushort)0); 
            bw.WriteZerosBytes(4);  
            bw.Write(SubObjects.Length);
            bw.EncodeArray(SubObjects);
        }

        // Write any remaining data
        Array.ForEach(RemainingData, bw.Write);
    }

    internal void Update(TmmModel tmmModel)
    {
        tmmModel.HeaderFile.Header.FileVersion = FileVersions.Latest;
        ModelInfo.Update(tmmModel);
    }
}
