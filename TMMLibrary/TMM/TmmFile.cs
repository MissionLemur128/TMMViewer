using System.Text;

namespace TMMLibrary.TMM;

public class TmmFile
{
    public TmmHeader Header { get; set; }
    public ModelInfo[] ModelInfo { get; set; }
    
    public static TmmFile Decode(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        return Decode(br);
    }

    public static TmmFile Decode(BinaryReader br)
    {
        var header = ParseHeader(br);

        br.BaseStream.Seek(header.DataOffset, SeekOrigin.Begin);
        var modelInfo = new ModelInfo[header.ModelCount];
        for (int i = 0; i < modelInfo.Length; ++i)
        {
            modelInfo[i] = ParseModelData(header, br);
        }    

        TmmFile tmmFile = new()
        {
            Header = header,
            ModelInfo = modelInfo
        };
        return tmmFile;
    }

    private static TmmHeader ParseHeader(BinaryReader br)
    {
        var header = new TmmHeader
        {
            MagicID = new string(br.ReadChars(4)),
            Unknown1 = br.ReadUInt32(),
            Unknown2 = br.ReadUInt16(),
            DataOffset = br.ReadUInt32(),
            ModelCount = br.ReadUInt32()
        };
        header.ModelNames = new string[header.ModelCount];

        for (int i = 0; i < header.ModelCount; i++)
        {
            var stringLen = br.ReadInt32(); // Encoded in unicode, so 2 bytes per char
            header.ModelNames[i] = br.ReadUtf16String(stringLen);
        }

        header.HeaderEndBytes = br.ReadUInt16();

        return header;
    }

    private static ModelInfo ParseModelData(TmmHeader header, BinaryReader br)
    {
        var modelInfo = new ModelInfo
        {
            Unknown1 = br.ReadUint16Array(4),
            Unknown2 = br.ReadUint16Array(35),
            BoneCount = br.ReadUInt32(),
            UnknownCount = br.ReadUInt32(),
            AttachPointCount = br.ReadUInt32(),
            VertexCount = br.ReadUInt32(),
            IndexCount = br.ReadUInt32(),
            VertexOffset = br.ReadUInt32(),
            IndexOffset = br.ReadUInt32(),
            IndexOffset2 = br.ReadUInt32(),
            Unknown3 = br.ReadUInt32(),
            BoneWeightsOffset = br.ReadUInt32(),
            BoneWeightsByteCount = br.ReadUInt32(),
            Unknown4 = br.ReadUint32Array(4),
            MaskDataOffset = br.ReadUInt32(),
            MaskDataByteCount = br.ReadUInt32(),
        };
        return modelInfo;
    }
}

/// <summary>
/// Describes the model information of a .tmm.data file.
/// </summary>
public class ModelInfo
{
    public ushort[] Unknown1 { get; set; } // always 2, 2, 6, 14 / 7, 2, 16, 17
    public ushort[] Unknown2 { get; set; }
    public uint BoneCount { get; set; }
    public uint UnknownCount { get; set; }
    public uint AttachPointCount { get; set; }
    public uint VertexCount { get; set; }
    public uint IndexCount { get; set; }
    public uint VertexOffset { get; set; } // Normally 0x00
    public uint IndexOffset { get; set; }
    public uint IndexOffset2 { get; set; } // Same as indexoffset, not sure why exists
    public uint Unknown3 { get; set; } // probably an offset of some sort
    public uint BoneWeightsOffset { get; set; }
    public uint BoneWeightsByteCount { get; set; }
    public uint[] Unknown4 { get; set; } // size of array is 4
    public uint MaskDataOffset { get; set; }
    public uint MaskDataByteCount { get; set; }
}

public class TmmHeader
{
    public string MagicID { get; set; }
    public uint Unknown1 { get; set; } // always 34
    public ushort Unknown2 { get; set; } // always 22
    public uint DataOffset { get; set; }
    public uint ModelCount { get; set; } // Unsure if this is correct
    public string[] ModelNames { get; set; }
    public ushort HeaderEndBytes { get; set; } // always 2024
}

public class Bone
{ 
    public string Name { get; set; }
    public uint BoneParent { get; set; }
    public float[] Unknown2 { get; set; } // size = 52
}
