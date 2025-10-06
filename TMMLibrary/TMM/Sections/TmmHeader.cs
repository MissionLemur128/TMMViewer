using TMMLibrary.Utils;

namespace TMMLibrary.TMM.Sections;

public class TmmHeader : IEncode
{
    public static readonly string TMM_MAGIC = "BTMM";
    public static readonly ushort MAGIC_DP = 0x5044;

    public string MagicId { get; set; } = ""; // always "BTMM"
    public uint FileVersion { get; set; }
    public ushort MagicDp { get; set; } // always 22
    public uint DataOffset { get; set; }
  
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
            FileVersion = br.ReadUInt32(),
            MagicDp = br.ReadUInt16(),
            DataOffset = br.ReadUInt32()
        };

        if (header.MagicDp != MAGIC_DP)
            throw new DecodeException(typeof(TmmHeader), "MagicDp wrong");

        return header;
    }

    public void Encode(BinaryWriter bw)
    {
        Array.ForEach(TMM_MAGIC.ToCharArray(), bw.Write);
        bw.Write(FileVersion);
        bw.Write(MagicDp);
        bw.Write(DataOffset);
    }
}
