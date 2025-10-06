using Microsoft.Xna.Framework;
using System.Numerics;

namespace TMMViewer.Data.Render.Nodes
{
    public class AttachPoint : ISceneNode
    {
        public override string Name { get; set; }

        public TMMLibrary.TMM.DataTypes.AttachPoint Data { get; }

        public AttachPoint(TMMLibrary.TMM.DataTypes.AttachPoint attachPoint)
        {
            RenderPriority = (int)RenderPriorityDefinitions.Foreground;
            Data = attachPoint;
            Name = $"{Data.Name1} : {Data.Name2}";
            Transform = attachPoint.LocalTransform;
        }

        public override void Render(RenderInfo info, Scene root, Matrix4x4 transform)
        {
            var _debugger = info.DebugDraw;
            const float radius = 0.25f;

            // Transform forward direction to world space
            var localForward = new System.Numerics.Vector4(0, 0, 1, 0);
            var worldForward4 = System.Numerics.Vector4.Transform(localForward, transform);
            var worldForward3 = new Microsoft.Xna.Framework.Vector3(worldForward4.X, worldForward4.Y, worldForward4.Z);

            var color = IsSelected ? Color.Yellow : Color.Red;
            _debugger.DrawRay(new Ray(transform.Translation, worldForward3), color, radius * 2f);

            _debugger.DrawWireSphere(new BoundingSphere(transform.Translation, radius), color);
        }
    }
}