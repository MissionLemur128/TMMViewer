using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TMMLibrary.Converters;
using TMMLibrary.TMM;
using TMMViewer.Data.Render;
using TMMViewer.ViewModels.MonoGameControls;

namespace TMMViewer.Data.Services
{
    public class IOModelService : IModelIOService
    {
        private Scene _scene;
        public TmmFile tmmFile = new();
        public List<TmmDataFile> tmmDataFiles = new();

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

            foreach (var model in tmmFile.ModelInfo)
            {
                var data = TmmDataFile.Decode(model, path + ".data");
                tmmDataFiles.Add(data);

                var vertices = data.Vertices.Select(
                    v => new VertexPositionNormalTexture(
                        new Vector3(v.Origin.X, v.Origin.Y, v.Origin.Z),
                        Vector3.Zero,
                        new Vector2(v.Uv.X, v.Uv.Y))
                    ).ToArray();
                SmoothNormals(data, ref vertices);
     
                var vertexBuffer = new VertexBuffer(graphicsDevice,
                    VertexPositionNormalTexture.VertexDeclaration,
                    vertices.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(vertices);

                var indexBuffer = new IndexBuffer(graphicsDevice,
                    IndexElementSize.SixteenBits, data.Indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(data.Indices);

                var material = _content.Load<Effect>("Effects/BasicShader");
                var meshObject = new Mesh(material, vertexBuffer, indexBuffer);
                _scene.Meshes.Add(meshObject);
            }
        }

        private static void SmoothNormals(TmmDataFile data, ref VertexPositionNormalTexture[] vertices)
        {
            for (int i = 0; i < data.Indices.Length; i += 3)
            {
                var index = data.Indices;
                var v0 = vertices[index[i]];
                var v1 = vertices[index[i + 1]];
                var v2 = vertices[index[i + 2]];

                var normal = Vector3.Cross(v1.Position - v0.Position, v2.Position - v0.Position);
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
                TmmAssimp.WriteToFile(tmmFile, mesh, path, format);
            }
        }
    }
}
