namespace TMMLibrary.TMM;

public class TmmFile
{
    public TmmHeader? Header { get; set; }
    public ModelInfo[] ModelInfos { get; set; } = [];
    
    public static TmmFile Decode(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        return Decode(br);
    }

    public static TmmFile Decode(BinaryReader br)
    {
        var header = TmmHeader.Decode(br);
        var modelInfo = br.DecodeArray((int)header.ModelCount, ModelInfo.Decode);
        DecodeException.ExpectEof(typeof(TmmFile), br.BaseStream);
        TmmFile tmmFile = new()
        {
            Header = header,
            ModelInfos = modelInfo
        };
        return tmmFile;
    }



    public void Encode(BinaryWriter bw)
    {
        Header?.Encode(bw);
        foreach (var it in ModelInfos)
        {
            it.Encode(bw);
        }
    }
}

public class TmmHeader : IEncode
{
    public static readonly string TMM_MAGIC = "BTMM";
    
    public string MagicId { get; set; } = "";
    public uint Unknown1 { get; set; } // always 34
    public ushort Unknown2 { get; set; } // always 22
    public uint DataOffset { get; set; }
    public uint ModelCount { get; set; } // Unsure if this is correct
    public string[] ModelNames { get; set; } = [];
    public ushort HeaderEndBytes { get; set; } // always 2024


    public static TmmHeader Decode(BinaryReader br)
    {
        var magic = new string(br.ReadChars(4));
        if (magic != TMM_MAGIC)
        {
            throw new DecodeException(typeof(TmmHeader), "incorrect magic header");
        }
        var header = new TmmHeader
        {
            MagicId = magic,
            Unknown1 = br.ReadUInt32(),
            Unknown2 = br.ReadUInt16(),
            DataOffset = br.ReadUInt32(),
            ModelCount = br.ReadUInt32()
        };
        header.ModelNames = br.DecodeArray((int)header.ModelCount, r => r.ReadTmString());
        header.HeaderEndBytes = br.ReadUInt16();
        
        return header;
    }

    public void Encode(BinaryWriter bw)
    {
        bw.Write(TMM_MAGIC);
        bw.Write(Unknown1);
        bw.Write(Unknown2);
        bw.Write(DataOffset);
        bw.Write(ModelCount);
        foreach (var s in ModelNames)
        {
            bw.WriteTmString(s);
        }
        bw.Write(HeaderEndBytes);
    }
}

/// <summary>
/// Describes the model information of a .tmm.data file.
/// </summary>
public class ModelInfo : IEncode
{
    public ushort[] Unknown1 { get; set; } = []; // always 2, 2, 6, 14 / 7, 2, 16, 17
    public ushort[] Unknown2 { get; set; } = [];
    public int BoneCount { get; set; }
    public uint UnknownCount { get; set; }
    public int AttachPointCount { get; set; }
    public uint VertexCount { get; set; }
    public uint IndexCount { get; set; }
    public uint VertexOffset { get; set; } // Normally 0x00
    public uint IndexOffset { get; set; }
    public uint IndexOffset2 { get; set; } // Same as indexoffset, not sure why exists
    public uint Unknown3 { get; set; } // probably an offset of some sort
    public uint BoneWeightsOffset { get; set; }
    public uint BoneWeightsByteCount { get; set; }
    public uint[] Unknown4 { get; set; } = []; // size of array is 4
   public uint MaskDataOffset { get; set; }
    public uint MaskDataByteCount { get; set; }
    // 0x34 bytes = 13 elements
    public float[] Unknown5 = [];
    public TmmAttachPoint[] AttachPoints = [];
    // 0x1c bytes = 7 elements, most are 0 but some have non-zero values that are probably not floats.
    public int[] Unknown6 = [];
    public string Material = "";
    // Appears directly after the material and always seems to be "default"
    public string SDefault = "";
    public Bone[] Bones = [];
    public uint[] UnknownEnd = [];
    
    public static ModelInfo Decode(BinaryReader br)
    {
        var modelInfo = new ModelInfo
        {
            Unknown1 = br.ReadUint16Array(4),
            Unknown2 = br.ReadUint16Array(35),
            BoneCount = br.ReadInt32(),
            UnknownCount = br.ReadUInt32(),
            AttachPointCount = br.ReadInt32(),
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
        
        // skip a byte to align. This is probably meaningful?
        br.ReadByte();
        // Another 0x34 bytes worth of floats
        modelInfo.Unknown5 = br.ReadFloat32Array(0x34 / 4);
        // Read all attach points
        modelInfo.AttachPoints = br.DecodeArray(modelInfo.AttachPointCount, TmmAttachPoint.Decode);
        // 0x1C worth of int32, most are 0 but there are some values in there.
        modelInfo.Unknown6 = br.ReadInt32Array(0x1C / 4);
        // Now positioned right before the material name
        modelInfo.Material = br.ReadTmString();
        modelInfo.SDefault = br.ReadTmString();
        // Read all the bones, because the bones are important!
        modelInfo.Bones = br.DecodeArray(modelInfo.BoneCount, Bone.Decode);
        modelInfo.UnknownEnd = br.ReadUint32Array(0x10 / 4); // 4 elements

        DecodeException.ExpectEqualList<uint>(
            typeof(ModelInfo), br.BaseStream.Position, [0, 0, 0x01585600, 0], modelInfo.UnknownEnd);
        return modelInfo;
    }

    public void Encode(BinaryWriter bw)
    {
        bw.Write(Unknown1);
        bw.Write(Unknown2);
        bw.Write(BoneCount);
        bw.Write(UnknownCount);
        bw.Write(AttachPointCount);
        bw.Write(VertexCount);
        bw.Write(IndexCount);
        bw.Write(VertexOffset);
        bw.Write(IndexOffset);
        bw.Write(IndexOffset2);
        bw.Write(Unknown3);
        bw.Write(BoneWeightsOffset);
        bw.Write(BoneWeightsByteCount);
        bw.Write(Unknown4);
        bw.Write(MaskDataOffset);
        bw.Write(MaskDataByteCount);
        bw.EncodeArray(AttachPoints);
    }
}

public class TmmAttachPoint : IEncode
{
    public int Flags;
    public string Name1 = "";
    // Second name, usually the same as the initial name when present but not always present.
    public string Name2 = "";
    public float[] Floats1 = [];
    
    public static TmmAttachPoint Decode(BinaryReader br)
    {
        var myType = typeof(TmmAttachPoint);
        var apoint = new TmmAttachPoint();
        
        // Expect 2x uint32 that are 0, then a flags uint32, then the name
        var zero0 = br.ReadUInt64();
        if (zero0 != 0)
        {
            throw new DecodeException(myType, br.BaseStream.Position - 8, "expected 8 zero bytes");
        }
        apoint.Flags = br.ReadInt32();
        apoint.Name1 = br.ReadTmString();
        // Start with 0x60 bytes of floats
        apoint.Floats1 = br.ReadFloat32Array(0x60 / 4);
        // Read 2x int32, should be 0
        var zero1 = br.ReadUInt64();
        DecodeException.ExpectEqual(myType, br.BaseStream.Position - 8, zero1, (ulong)0);
        // Next value is always a TmString, but it may be zero-length.
        apoint.Name2 = br.ReadTmString();
        // Ends with [-1, 0, 0]
        var tmp1 = br.ReadInt32Array(3);
        DecodeException.ExpectEqualList(myType, br.BaseStream.Position, [-1, 0, 0], tmp1);
        return apoint;
    }

    public void Encode(BinaryWriter bw)
    {
        bw.Write((ulong)0);
        bw.Write(Flags);
        bw.WriteTmString(Name1);
        bw.Write(Floats1);
        bw.Write((ulong)0); // 8 bytes of 0s
        bw.WriteTmString(Name2);
        // So far, these are constant
        bw.Write(-1);
        bw.Write(0);
        bw.Write(0);
    }
}

public class Bone : IEncode
{
    public string Name = "";
    public int BoneParent;
    public float[] Unknown2 = []; // size = 52

    public static Bone Decode(BinaryReader br)
    {
        return new Bone
        {
            Name = br.ReadTmString(),
            BoneParent = br.ReadInt32(),
            Unknown2 = br.ReadFloat32Array(0xD0 / 4), // = 52
        };
    }
    
    public void Encode(BinaryWriter bw)
    {
        bw.WriteTmString(Name);
        bw.Write(BoneParent);
        bw.Write(Unknown2);
    }
}
