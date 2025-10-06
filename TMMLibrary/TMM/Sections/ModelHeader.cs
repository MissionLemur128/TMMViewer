using TMMLibrary.Utils;

namespace TMMLibrary.TMM.Sections;

public class ModelHeader : IEncode
{
    public string ModelName { get; set; }
    public ushort Year { get; set; }
    public ushort[] Unknown { get; set; }

    public static ModelHeader Decode(BinaryReader br)
    {
        var header = new ModelHeader();
        header.ModelName = br.ReadTmString();
        header.Year = br.ReadUInt16();
        header.Unknown = br.ReadUint16Array(7);
        return header;
    }

    public void Encode(BinaryWriter bw)
    {
        bw.WriteTmString(ModelName);
        bw.Write(Year);
        Array.ForEach(Unknown, bw.Write);
    }
}
