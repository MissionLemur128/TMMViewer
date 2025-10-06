using TMMViewer.Data.Render.Nodes;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace TMMViewer.Data.Render
{
    public partial class Scene
    {
        public class RenderNode()
        {                                                                                                                             
            public int Priority { get; set; }
            public Matrix4x4 Transform { get; set; }
            public required ISceneNode Node { get; set; }
        }
    }
}