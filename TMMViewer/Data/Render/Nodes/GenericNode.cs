using Matrix4x4 = System.Numerics.Matrix4x4;

namespace TMMViewer.Data.Render.Nodes
{
    public class GenericNode : ISceneNode
    {
        public override string Name { get; set; } = string.Empty;

        public override void Render(RenderInfo info, Scene scene, Matrix4x4 worldTransform){}
    }
}