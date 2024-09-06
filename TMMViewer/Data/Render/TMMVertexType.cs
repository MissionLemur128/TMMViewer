using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace TMMViewer.Data.Render
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TMMVertexType : IVertexType
    {
        [DataMember]
        public Vector3 Position;

        [DataMember]
        public Vector3 Normal;

        [DataMember]
        public Vector2 TextureCoordinate;

        [DataMember]
        public Vector4 BoneWeights;

        [DataMember]
        public Color BoneIndices;

        [DataMember]
        public Color Mask;

        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public TMMVertexType(
            Vector3 position,
            Vector3 normal,
            Vector2 textureCoordinate,
            Vector4 boneWeights,
            byte[] boneIndices,
            Color mask)
        {
            Position = position;
            Mask = mask;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            BoneWeights = boneWeights;
            BoneIndices = new Color(boneIndices[0], boneIndices[1], boneIndices[2], boneIndices[3]);
        }

        static TMMVertexType()
        {
            VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                new VertexElement(48, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0),
                new VertexElement(52, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );
        }
    }

}
