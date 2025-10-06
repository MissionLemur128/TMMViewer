using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using TMMLibrary;
using TMMLibrary.TMM.DataTypes;
using TMMLibrary.TMMData;
using Color = Microsoft.Xna.Framework.Color;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;


namespace TMMViewer.Data.Render.Nodes
{
    public class Mesh : ISceneNode
    {
        public override string Name { get; set; }

        private readonly Effect _material;
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indices;

        private Vector3 _uniqueColor;

        public Mesh(GraphicsDevice graphicsDevice, ContentManager content, TmmModel model, MaterialVertexGroup material)
        {
            Name = model.HeaderFile.Materials[material.MaterialIndex];
            var data = model.DataFile;
            var vertices = new TMMVertexType[material.VertexCount];
            _uniqueColor = ColorCollection.GetColorForIndex((int)material.MaterialIndex).ToVector3();

            for (var i = 0; i < vertices.Length; ++i)
            {
                var adjustedIndex = i + material.VertexOffset;
                var vertex = data.Vertices[adjustedIndex];
                var gradient = new Color(
                        (byte)(data.HeightGradient[i * 2]),
                        (byte)(data.HeightGradient[i * 2 + 1]),
                        (byte)(0));

                var hasBones = model.HeaderFile.Bones.Length > 0;
                var boneWeights2 = Vector4.Zero;
                byte[] boneIndices = [];
                if (hasBones)
                {
                    var boneWeights = data.BoneWeights[i];
                    boneIndices = boneWeights.BoneIndices;
                    if (boneWeights.Weights.Length >= 4)
                    {
                        boneWeights2 = new Vector4(boneWeights.Weights[0], boneWeights.Weights[1], boneWeights.Weights[2], boneWeights.Weights[3]) / 100f;
                    }
                }

                var _uniquePartColor = Color.LightGray;
                if (data.SubObjectIds.Length > 0)
                {
                    var partId = data.SubObjectIds.Length > 0 ? data.SubObjectIds[adjustedIndex].ObjectId : 0;
                    _uniquePartColor = ColorCollection.GetColorForIndex(partId);
                }

                vertices[i] = new TMMVertexType(
                    vertex.Origin,
                    vertex.Normal,
                    vertex.Tangent,
                    vertex.Uv,
                    boneWeights2,
                    boneIndices,
                    gradient,
                    _uniquePartColor);
            }

            _vertexBuffer = new VertexBuffer(graphicsDevice,
                  TMMVertexType.VertexDeclaration,
                  vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertices);

            var indices = new ushort[material.IndexCount];
            Array.Copy(data.Indices, material.IndexOffset, indices, 0, indices.Length);
            _indices = new IndexBuffer(graphicsDevice,
                IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            _indices.SetData(indices);

            _material = content.Load<Effect>("Effects/DefaultShader");
        }

        public override void Render(RenderInfo info, Scene root, Matrix4x4 transform)
        {
            var graphicsDevice = _vertexBuffer.GraphicsDevice;
            graphicsDevice.Indices = _indices;
            graphicsDevice.SetVertexBuffer(_vertexBuffer);

            _material.Parameters["_world"].SetValue(transform);
            _material.Parameters["_diffuseColor"].SetValue(GetColor(root));

            root.Camera.ApplyToMaterial(_material);
            root.DirectionalLight.ApplyToMaterial(_material);
            root.Environment.ApplyToMaterial(_material);

            SetTechnique(root.RenderMode);
            foreach (var pass in _material.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0, _indices.IndexCount / 3);
            }
        }

        private Vector3 GetColor(Scene root)
        {
            if (root.RenderMode == RenderMode.Materials)
                return _uniqueColor;
            else
                return Color.LightGray.ToVector3();
        }

        private void SetTechnique(RenderMode mode)
        {
            _material.CurrentTechnique = mode switch
            {
                RenderMode.Normals => _material.Techniques["Normals"],
                RenderMode.Tangents => _material.Techniques["Tangents"],
                RenderMode.Bitangents => _material.Techniques["Bitangents"],
                RenderMode.Mask => _material.Techniques["Mask"],
                RenderMode.BoneWeights => _material.Techniques["BoneWeights"],
                RenderMode.SubObjectIds => _material.Techniques["ObjectIds"],
                _ => _material.Techniques["Solid"],
            };
        }
    }
}
