using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TMMLibrary.Converters;
using TMMLibrary.TMM;
using TMMViewer.Data.Render;
using TMMViewer.ViewModels.MonoGameControls;
using GLVector3 = Microsoft.Xna.Framework.Vector3;
using System.IO;
using SharpDX.MediaFoundation;

namespace TMMViewer.Data.Services
{
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

            foreach (var model in tmmFile.ModelInfos)
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
                    var boneWeights2 = Vector4.Zero;
                    if (boneWeights.Weights.Length >= 4)
                    {
                        boneWeights2 = new Vector4(boneWeights.Weights[0], boneWeights.Weights[1], boneWeights.Weights[2], boneWeights.Weights[3]) / 100f;
                    }

                    vertices[i] = new TMMVertexType(
                        new Vector3(-v.Origin.X, v.Origin.Y, v.Origin.Z),
                        normal,
                        new Vector2(v.Uv.X, v.Uv.Y),
                        boneWeights2,
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
            //}

            //foreach (var model in tmmFile.ModelInfos)
            //{
                Render.Bone.InitGlobal(_monoGame.GraphicsDevice, _content);
                var skeleton = new Skeleton();
                var id = 0;
                foreach (var bone in model.Bones)
                {
                    if (skeleton.Bones.ContainsKey((ushort)id))
                        continue;

                    var bone3D = new Render.Bone();



                    //var v = new Vector4[4];
                    //var offset = 64;
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    v[i] = new Vector4(
                    //        BitConverter.ToSingle(bone.Unknown2, offset + i * 16),
                    //        BitConverter.ToSingle(bone.Unknown2, offset + i * 16 + 4),
                    //        BitConverter.ToSingle(bone.Unknown2, offset + i * 16 + 8),
                    //        BitConverter.ToSingle(bone.Unknown2, offset + i * 16 + 12));
                    //}

                    var v = new Vector3[3];
                    var offset = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        v[i] = new Vector3(
                            BitConverter.ToSingle(bone.Unknown2, offset + i * 12),
                            BitConverter.ToSingle(bone.Unknown2, offset + i * 12 + 4),
                            BitConverter.ToSingle(bone.Unknown2, offset + i * 12 + 8));
                    }


                    //bone3D.Transform = new Matrix(v[0], v[1], v[2], v[3]);
                    //bone3D.Transform = Matrix.CreateWorld(v[0], v[1], v[2]);
                    bone3D.data = bone.Unknown2;
                    bone3D.parent = bone.BoneParent;
                    bone3D.index = id;

                        //Matrix.CreateScale(bone.Scale) *
                        //Matrix.CreateFromQuaternion(bone.Rotation) *
                        //Matrix.CreateTranslation(bone.Position);

                    skeleton.Bones.Add((ushort)id, bone3D);
                    id++;
                }
                _scene.Skeleton = skeleton;
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
