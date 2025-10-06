using Microsoft.Xna.Framework;
using System.Numerics;
using TMMLibrary.TMM.Sections;

namespace TMMViewer.Data.Render.Nodes
{
    public class BoundingVolume(ModelBoundingInfo modelInfo) : ISceneNode
    {
        public override string Name { get; set; } = "Bounding Volume";
            
        public override void Render(RenderInfo info, Scene scene, Matrix4x4 transform)
        {
            var _debugger = info.DebugDraw;

            var color = IsSelected ? Color.Yellow : Color.Red;

            _debugger.DrawWireBox(new BoundingBox(modelInfo.BoundingBoxes.Min, modelInfo.BoundingBoxes.Max), color);
            _debugger.DrawWireBox(new BoundingBox(modelInfo.LooseBoundingBoxes.Min, modelInfo.LooseBoundingBoxes.Max), color);

            _debugger.DrawWireSphere(new BoundingSphere((modelInfo.BoundingBoxes.Min + modelInfo.BoundingBoxes.Max) / 2, modelInfo.BoundingRadius), IsSelected ? Color.Yellow : Color.Cyan);
        }
    }
}
