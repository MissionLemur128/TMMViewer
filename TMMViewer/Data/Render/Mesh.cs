using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TMMViewer.Data.Render
{
    public class Mesh
    {
        public Effect Material { get; set; }

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indices;

        public Mesh(Effect material, VertexBuffer vertexBuffer, IndexBuffer indices)
        {
            Material = material;
            _vertexBuffer = vertexBuffer;
            _indices = indices;
        }

        public void Render(RenderMode mode)
        {
            var graphicsDevice = _vertexBuffer.GraphicsDevice;
            graphicsDevice.Indices = _indices;
            graphicsDevice.SetVertexBuffer(_vertexBuffer);

            Material.Parameters["_world"].SetValue(Matrix.Identity);
            Material.Parameters["_diffuseColor"].SetValue(Color.LightGray.ToVector3());

            SetTechnique(mode);
            foreach (var pass in Material.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0, _indices.IndexCount / 3);
            }
        }

        private void SetTechnique(RenderMode mode)
        {
            switch (mode)
            {
                case RenderMode.Normals:
                    Material.CurrentTechnique = Material.Techniques["Normals"];
                    break;
                case RenderMode.Mask:
                    Material.CurrentTechnique = Material.Techniques["Mask"];
                    break;
                case RenderMode.BoneWeights:
                    Material.CurrentTechnique = Material.Techniques["BoneWeights"];
                    break;
                default:
                    Material.CurrentTechnique = Material.Techniques["Solid"];
                    break;
            }
        }
    }
}
