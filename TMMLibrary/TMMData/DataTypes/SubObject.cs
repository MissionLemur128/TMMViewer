using TMMLibrary.Utils;

namespace TMMLibrary.TMMData.DataTypes
{
    public struct SubObject : IEncode
    {
        public const uint Stride = 2 * sizeof(byte);

        public byte Unknown { get; set; }
        public byte ObjectId { get; set; }

        public static SubObject Decode(BinaryReader br)
        {
            return new SubObject
            {
                ObjectId = br.ReadByte(),
                Unknown = br.ReadByte()
            };
        }

        public void Encode(BinaryWriter bw)
        {
            bw.Write(ObjectId);
            bw.Write(Unknown);
        }
    }
}
