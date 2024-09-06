using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TMMLibrary.Converters;
using TMMLibrary.TMM;
using TMMViewer.Data.Render;
using TMMViewer.ViewModels.MonoGameControls;
using TmmVector3 = System.Numerics.Vector3;
using TmmVector2 = System.Numerics.Vector2;
using GLVector4 = Microsoft.Xna.Framework.Vector4;
using GLVector3 = Microsoft.Xna.Framework.Vector3;
using GLVector2 = Microsoft.Xna.Framework.Vector2;
using GLColor = Microsoft.Xna.Framework.Color;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace TMMViewer.Data.Services
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TMMVertexType : IVertexType
    {
        [DataMember]
        public GLVector3 Position;

        [DataMember]
        public GLVector3 Normal;

        [DataMember]
        public GLVector2 TextureCoordinate;

        [DataMember]
        public GLVector4 BoneWeights;

        [DataMember]
        public GLColor BoneIndices;

        [DataMember]
        public GLColor Mask;

        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public TMMVertexType(
            GLVector3 position, 
            GLVector3 normal, 
            GLVector2 textureCoordinate, 
            GLVector4 boneWeights,
            byte[] boneIndices,
            GLColor mask)
        {
            Position = position;
            Mask = mask;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            BoneWeights = boneWeights;
            BoneIndices = new GLColor(boneIndices[0], boneIndices[1], boneIndices[2], boneIndices[3]);
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

    public class IOModelService : IModelIOService
    {
        private Scene _scene;
        public TmmFile tmmFile = new();
        public List<TmmDataFile> tmmDataFiles = new();
        public string? OpenedModelPath { get; private set; }

        private IMonoGameViewModel _monoGame;

        public IOModelService(Scene scene, IMonoGameViewModel monoGame)
        {
            _scene = scene;
            _monoGame = monoGame;
        }

        public void ImportModel(string path)
        {
            var graphicsDevice = _monoGame.GraphicsDeviceService.GraphicsDevice;
            var _content = _monoGame.Content;

            _scene.Meshes.Clear();
            tmmDataFiles.Clear();
            tmmFile = TmmFile.Decode(path);
            OpenedModelPath = path;

            var yMax = float.MinValue;
            var yMin = float.MaxValue;

            ushort[] iMax = { ushort.MinValue, ushort.MinValue, ushort.MinValue };
            ushort[] iMin = { ushort.MaxValue, ushort.MaxValue, ushort.MaxValue };

            foreach (var model in tmmFile.ModelInfo)
            {
                var data = TmmDataFile.Decode(model, path + ".data");
                tmmDataFiles.Add(data);
                
                var vertices = new TMMVertexType[data.Vertices.Length];
                for (int i = 0; i < data.Vertices.Length; ++i)
                {
                    var v = data.Vertices[i];
                    var boneWeights = data.BoneWeights[i];
                    var mask = new Color(
                            (byte)(data.Mask[i * 2]),
                            (byte)(data.Mask[i * 2 + 1]),
                            (byte)(0));

                var halfValue = short.MaxValue / 2;
                    var normal = new GLVector3(
                            halfValue - BitConverter.ToInt16(v.Unknown0, 0),
                            halfValue - BitConverter.ToInt16(v.Unknown0, 2),
                            BitConverter.ToInt16(v.Unknown0, 4) - halfValue);
                    normal.Normalize();
                    vertices[i] = new TMMVertexType(
                        new Vector3(-v.Origin.X, v.Origin.Y, v.Origin.Z),
                        normal,
                        new Vector2(v.Uv.X, v.Uv.Y),
                        new Vector4(boneWeights.Weights[0], boneWeights.Weights[1], boneWeights.Weights[2], boneWeights.Weights[3]),
                        boneWeights.BoneIndices, 
                        mask );
                }
               
                foreach (var vertex in vertices)
                {
                    yMax = Math.Max(yMax, vertex.Position.Y);
                    yMin = Math.Min(yMin, vertex.Position.Y);
                }

                var vertexBuffer = new VertexBuffer(graphicsDevice,
                    TMMVertexType.VertexDeclaration,
                    vertices.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(vertices);

                var indices = data.Indices.ToArray();
                Array.Reverse(indices);
                var indexBuffer = new IndexBuffer(graphicsDevice,
                    IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices);

                var material = _content.Load<Effect>("Effects/DefaultShader");
                var meshObject = new Mesh(material, vertexBuffer, indexBuffer);
                _scene.Meshes.Add(meshObject);
            }

            _scene.Camera.Target = new Vector3(0, (yMax + yMin) / 2, 0);
        }

        public void ExportModel(string path, string format)
        {
            foreach (var mesh in tmmDataFiles)
            {
                var success = TmmAssimp.WriteToFile(tmmFile, mesh, path, format);
            }
        }

        public void ExportModelDebug(string path)
        {
            foreach (var mesh in tmmDataFiles)
            {
                using MemoryStream stream = new();
                using BinaryWriter writer = new(stream);
                mesh.Encode(writer);

                var data = stream.ToArray();
                File.WriteAllBytes(path, data);
            }
        }
    }
}
