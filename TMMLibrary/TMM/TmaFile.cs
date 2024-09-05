namespace TMMLibrary.TMM;

public class TmaFile
{
    public TmaHeader? FileHeader = null;
    public TmaAnimHeader[] Headers = [];
    public TmaAnimData[] Datas = [];

    public static TmaFile Decode(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        return Decode(br);
    }
    
    public static TmaFile Decode(BinaryReader br)
    {
        var header = TmaHeader.Decode(br);
        var anims = br.DecodeArray((int)header.AnimCount2, TmaAnimHeader.Decode);
        var datas = br.DecodeArray((int)header.AnimCount1, TmaAnimData.Decode);
        // Should be 8 more null bytes and then EOF
        var expectZero = br.ReadUInt64();
        if (expectZero != 0)
        {
            throw new DecodeException(typeof(TmaFile), "expected last 8 bytes to be 0");
        }
        DecodeException.ExpectEof(typeof(TmaFile), br.BaseStream);
        return new TmaFile
        {
            FileHeader = header,
            Headers = anims,
            Datas = datas,
        };
    }
}

public class TmaHeader
{
    // 4 bytes, probably version, always 12
    public uint Unknown1;
    // 2 bytes, always the string "DP"
    public ushort UnknownDP;
    // Unknown, seen 326 (reginleif_heal_a.tma), 350 (fott_regi_26_thats_right.tma)
    // and 354 (fott_regi_27b_dont_let_the.tma). Might be number of animation frames?
    public uint Unknown2;
    // Unknown, always 3 in Reginleif anims
    public uint Unknown3;
    // Seems like metadata about the importer used
    public string FbxImportPath = "";
    // 0x10 bytes
    public byte[] Unknown4 = [];
    // Path (relative to root) of the main .fbx file this is an animation for
    public string ModelFbxPath = "";
    // 0x10 bytes
    public byte[] Unknown5 = [];
    // Path (relative to root) of the main .fbx file this is an animation for with "import" at the end
    public string ModelFbxImportPath = "";
    // 0x10 bytes
    public byte[] Unknown6 = [];
    // First count of animation headers. In some of the Reginleif animations it's one less than the second count,
    // but in the contarius ones it's equal to the second count. For the Reginleif files, the second count seems
    // correct, so I'd guess the first count is for something else, but related.
    public uint AnimCount1;
    // 0x20 bytes
    public byte[] Unknown7 = [];
    // See AnimCount1
    public uint AnimCount2;
    
    
    public static TmaHeader Decode(BinaryReader br)
    {
        var magic = br.ReadUInt32();
        if (magic != 0x414d5442) // "BTMA"
        {
            throw new IOException(".tma invalid magic");
        }

        var header = new TmaHeader
        {
            Unknown1 = br.ReadUInt32(),
            UnknownDP = br.ReadUInt16(),
            Unknown2 = br.ReadUInt32(),
            Unknown3 = br.ReadUInt32(),
            FbxImportPath = br.ReadTmString(),
            Unknown4 = br.ReadBytes(16),
            ModelFbxPath = br.ReadTmString(),
            Unknown5 = br.ReadBytes(16),
            ModelFbxImportPath = br.ReadTmString(),
            Unknown6 = br.ReadBytes(16),
            AnimCount1 = br.ReadUInt32(),
            Unknown7 = br.ReadBytes(0x20),
            AnimCount2 = br.ReadUInt32(),
        };
        var expectZero = br.ReadUInt32();
        if (header.UnknownDP != 0x5044) // "DP"
        {
            throw new IOException(".tma invalid second-magic");
        }
        if (expectZero != 0)
        {
            throw new IOException($".tma expected 0 before animation headers, found 0x{expectZero:x}");
        }

        return header;
    }
}

public class TmaAnimHeader
{
    public string Name = "";
    // A single uint32, seems to be an index, with the "root" object having index 0xFFFFFFFF (-1).
    // The weird thing is in the contarius_horse_birth_a.tma file the "Root1" section has index -1,
    // while both "Rider" and "Root" have index 0, followed by "Spine" with index 2. Maybe "Index" isn't quite right?
    public uint Index;
    // 48 floats
    // These have lots of 0s, which makes it hard to tell but the other values
    // in this section often end up being 1.0f, 1.5f, or other floats in the range 0-1 with otherwise
    // nonsense uint32 values.
    public float[] Unknown48 = [];

    public static TmaAnimHeader Decode(BinaryReader br)
    {
        var head = new TmaAnimHeader
        {
            Name = br.ReadTmString(),
            Index = br.ReadUInt32(),
            Unknown48 = br.ReadFloat32Array(48),
        };
        if (head.Name.Length > 30)
        {
            throw new IOException("stop here!");
        }

        return head;
    }
}

public struct TmaAnimData
{
    public string Name;
    // These appear to be 4 bytes, maybe indicating what type of data this animation frame contains?
    // Known values: 1
    public byte B0;
    // Known values: 0, 1
    public byte B1;
    // Known values: 0, 3
    public byte B2;
    // Known values: 0
    public byte B3;
    // This is not a length value. No idea what it's for, but it's consistently there.
    public uint Unknown1;
    // I have no idea how to determine length, but sometimes there seems to be a uint32 length indicator
    // following B3, but other times there isn't. Not sure what to make of it yet.
    // Update: my first attempt is below.

    // There seem to be groups of floats, with not all of them showing up all the time.
    public float[] Floats1;
    public float[] Floats2;
    public float[] Floats3;

    public static TmaAnimData Decode(BinaryReader br)
    {
        var data = new TmaAnimData();
        data.DecodeImpl(br);
        return data;
    }

    private void DecodeImpl(BinaryReader br)
    {
        var myType = typeof(TmaAnimData);
        var pos = br.BaseStream.Position;
        Name = br.ReadTmString();
        B0 = br.ReadByte();
        B1 = br.ReadByte();
        B2 = br.ReadByte();
        B3 = br.ReadByte();
        if (B0 != 1 || B3 != 0)
        {
            throw new DecodeException(myType, pos, "unexpected values, refusing to decode more");
        }

        Unknown1 = br.ReadUInt32();

        switch (B1)
        {
            case 0:
                Floats1 = br.ReadFloat32Array(0x10 / 4);
                break;
            case 1:
            {
                var len1 = br.ReadInt32();
                Floats1 = br.ReadFloat32Array(len1 / 4);
                break;
            }
            default:
                throw new DecodeException(myType, pos, $"B1 is unknown value {B1}, refusing to continue");
        }

        switch (B2)
        {
            case 0:
                Floats2 = br.ReadFloat32Array(0x10 / 4);
                break;
            case 3:
                var len2 = br.ReadInt32();
                Floats2 = br.ReadFloat32Array(len2 / 4);
                break;
            default:
                throw new DecodeException(myType, pos, $"B2 is unknown value {B2}, refusing to continue");
        }
        
        // And the ending 0x10 bytes worth of floats
        Floats3 = br.ReadFloat32Array(0x10 / 4);
    }
}
