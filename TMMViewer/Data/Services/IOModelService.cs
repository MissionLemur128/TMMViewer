using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TMMLibrary.Converters;
using TMMLibrary.TMM;
using TMMViewer.Data.Render;
using TMMViewer.ViewModels.MonoGameControls;
using TmmVector3 = System.Numerics.Vector3;
using TmmVector2 = System.Numerics.Vector2;
using System.IO;

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

            ushort[] iMax = { ushort.MinValue, ushort.MinValue, ushort.MinValue };
            ushort[] iMin = { ushort.MaxValue, ushort.MaxValue, ushort.MaxValue };

            foreach (var model in tmmFile.ModelInfo)
            {
                var data = TmmDataFile.Decode(model, path + ".data");
                tmmDataFiles.Add(data);

                var vertices = data.Vertices.Select(
                    v =>
                    {
                        var tangent = v.Tangent;
                        tangent.X = -tangent.X;
                        var normal = Vector3.Cross(Vector3.Right, tangent);
                        normal.Normalize();
                        return new VertexPositionColorNormalTexture(
                            new Vector3(-v.Origin.X, v.Origin.Y, v.Origin.Z),
                            new Color(),
                            tangent,
                            new Vector2(v.Uv.X, v.Uv.Y));
                    }).ToArray();

                for (int i = 0; i < data.Vertices.Length; ++i)
                {
                    var vertex = vertices[i];
                    vertex.Color = new Color(
                    (byte)(data.Mask[i * 2]),
                    (byte)(data.Mask[i * 2 + 1]),
                    (byte)(0));
                    vertices[i] = vertex;
                }

                //SmoothNormals(data, ref vertices);

                foreach (var vertex in vertices)
                {
                    yMax = Math.Max(yMax, vertex.Position.Y);
                    yMin = Math.Min(yMin, vertex.Position.Y);
                }

                //foreach (var vertex in data.Vertices)
                //{
                //    iMax[0] = Math.Max(iMax[0], vertex.Unknown0);
                //    iMin[0] = Math.Min(iMin[0], vertex.Unknown0);
                //    iMax[1] = Math.Max(iMax[1], vertex.Unknown1);
                //    iMin[1] = Math.Min(iMin[1], vertex.Unknown1);
                //    iMax[2] = Math.Max(iMax[2], vertex.Unknown2);
                //    iMin[2] = Math.Min(iMin[2], vertex.Unknown2);
                //}

                var vertexBuffer = new VertexBuffer(graphicsDevice,
                    VertexPositionColorNormalTexture.VertexDeclaration,
                    vertices.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(vertices);

                var indices = data.Indices.ToArray();
                Array.Reverse(indices);
                var indexBuffer = new IndexBuffer(graphicsDevice,
                    IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices);

                var material = _content.Load<Effect>("Effects/BasicShader");
                var meshObject = new Mesh(material, vertexBuffer, indexBuffer);
                _scene.Meshes.Add(meshObject);
            }

            _scene.Camera.Target = new Vector3(0, (yMax + yMin) / 2, 0);
        }

        private static void SmoothNormals(TmmDataFile data, ref VertexPositionNormalTexture[] vertices)
        {
            for (int i = 0; i < data.Indices.Length; i += 3)
            {
                var index = data.Indices;
                var v0 = vertices[index[i]];
                var v1 = vertices[index[i + 1]];
                var v2 = vertices[index[i + 2]];

                var normal = -Vector3.Cross(v1.Position - v0.Position, v2.Position - v0.Position);
                normal.Normalize();

                v0.Normal += normal;
                v1.Normal += normal;
                v2.Normal += normal;

                vertices[index[i]] = v0;
                vertices[index[i + 1]] = v1;
                vertices[index[i + 2]] = v2;
            }
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
                //ushort l = 0;
                //mesh.Vertices = mesh.Vertices.Select(v =>
                //{
                //    var normal = v.Origin;
                //    var max = Math.Max(Math.Max(normal.X, normal.Y), normal.Z);
                //    if (max != 0)
                //    {
                //        normal /= max;
                //    }

                //    return new TmmVertex
                //    {
                //        Origin = v.Origin,
                //        Uv = v.Uv,
                //        Tangent = TmmVector3.Zero,
                //        //Unknown0 = v.Unknown0,
                //        //Unknown1 = v.Unknown1,
                //        //Unknown2 = 0,
                //    };
                //    }
                //).ToArray();

                mesh.Mask = mesh.Mask.Select(v => byte.MaxValue).ToArray();

                using MemoryStream stream = new();
                using BinaryWriter writer = new(stream);
                mesh.Encode(writer);

                var data = stream.ToArray();
                File.WriteAllBytes(path, data);
            }
        }
    }
}
