using TMMLibrary.Utils;

namespace TMMLibrary.TMM.DataTypes
{
    public class MaterialVertexGroup : IEncode
    {
        public int VertexOffset { get; set; }
        public int IndexOffset { get; set; }
        public int VertexCount { get; set; }
        public int IndexCount { get; set; }
        public int MaterialIndex { get; set; }
        public int ShaderIndex { get; set; }

        public static MaterialVertexGroup Decode(BinaryReader br)
        {
            return new MaterialVertexGroup()
            {
                VertexOffset = br.ReadInt32(),
                IndexOffset = br.ReadInt32(),
                VertexCount = br.ReadInt32(),
                IndexCount = br.ReadInt32(),
                MaterialIndex = br.ReadInt32(),
                ShaderIndex = br.ReadInt32(),
            };
        }

        public void Encode(BinaryWriter bw)
        {
            bw.Write(VertexOffset);
            bw.Write(IndexOffset);
            bw.Write(VertexCount);
            bw.Write(IndexCount);
            bw.Write(MaterialIndex);
            bw.Write(ShaderIndex);
        }
    }
}
