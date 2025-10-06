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
        public Vector4 Tangent;

        [DataMember]
        public Vector2 TextureCoordinate;

        [DataMember]
        public Vector4 BoneWeights;

        [DataMember]
        public Color BoneIndices;

        [DataMember]
        public Color Mask;

        [DataMember]
        public Color SubObjectId;

        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public TMMVertexType(
            Vector3 position,
            Vector3 normal,
            Vector4 tangent,
            Vector2 textureCoordinate,
            Vector4 boneWeights,
            byte[] boneIndices,
            Color mask,
            Color subObjectId)
        {
            Position = position;
            Mask = mask;
            SubObjectId = subObjectId;
            Normal = normal;
            Tangent = tangent;
            TextureCoordinate = textureCoordinate;
            BoneWeights = boneWeights;
            if (boneIndices.Length == 4)
            {
                BoneIndices = new Color(boneIndices[0], boneIndices[1], boneIndices[2], boneIndices[3]);
            }

        }

        static TMMVertexType()
        {
            VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), // position
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), // normal
                new VertexElement(24, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0), // tangent
                new VertexElement(40, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0), // uv
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0), // boneWeights
                new VertexElement(64, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0), // boneIndices
                new VertexElement(68, VertexElementFormat.Color, VertexElementUsage.Color, 0), // Gradient Mask
                new VertexElement(72, VertexElementFormat.Color, VertexElementUsage.Color, 0) // SubObjectId
            );
        }
    }
}
