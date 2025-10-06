using System.Numerics;
using TMMLibrary.Utils;

namespace TMMLibrary.TMM.DataTypes
{
    public class SubObject : IEncode
    {
        public string Name { get; set; }
        public Matrix4x4 WorldTransform { get; set; }
        public Matrix4x4 InverseWorldTransform { get; set; }

        public static SubObject Decode(BinaryReader br)
        {
            return new()
            {
                Name = br.ReadTmString(),
                WorldTransform = br.ReadMatrix4x4(),
                InverseWorldTransform = br.ReadMatrix4x4(),
            };
        }

        public void Encode(BinaryWriter bw)
        {
            bw.WriteTmString(Name);
            bw.Write(WorldTransform);
            bw.Write(InverseWorldTransform);
        }
    }
}