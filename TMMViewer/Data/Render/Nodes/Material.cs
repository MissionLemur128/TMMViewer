using System.Numerics;
using TMMLibrary.TMM;
using TMMLibrary.TMM.DataTypes;
namespace TMMViewer.Data.Render.Nodes
{
    public class Material(TmmFile file, MaterialVertexGroup materialVertexGroup) : ISceneNode
    {
        public override string Name { get; set; } = file.Materials[materialVertexGroup.MaterialIndex];
        public int ShaderTechnique { get; set; }

        public override void Render(RenderInfo info, Scene scene, Matrix4x4 transform)
        {
        }
    }
}
