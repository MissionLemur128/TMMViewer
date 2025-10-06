using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using TMMViewer.Data.Render.Cameras;
using TMMViewer.Data.Render.Debug;
using TMMViewer.Data.Render.Nodes;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace TMMViewer.Data.Render
{
    public partial class Scene
    {
        public RenderMode RenderMode { get; set; } = RenderMode.Solid;

        public Environment Environment { get; set; } = new();

        public Camera Camera { get; set; } = new OrbitCamera();

        public DirectionalLight DirectionalLight { get; set; } = new();

        public ObservableCollection<ISceneNode> Children { get; } = [];

        public ObservableCollection<Material> Materials { get; set; } = [];

        public static Scene CreateEmpty()
        {
            return new Scene();
        }

        private Scene()
        {
            Camera = new OrbitCamera();
            DirectionalLight = new DirectionalLight();
            Environment = new Environment();
            RenderMode = RenderMode.Solid;
        }

        public void Render(RenderInfo info)
        {
            var device = info.GraphicsDevice;
            var debugger = info.DebugDraw;
            Camera.AspectRatio = device.Viewport.AspectRatio;
            device.Clear(Camera.BackgroundColor);

            RenderGrid(debugger, device);

            debugger.Begin(Camera.View, Camera.Projection);
            var renderQueue = new List<RenderNode>();
            var transform = Matrix4x4.Identity;
            QueueRenderObjects(Children, renderQueue, info, transform);
            renderQueue.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            foreach (var tuple in renderQueue)
            {
                if (tuple.Priority >= (int)RenderPriorityDefinitions.Foreground)
                {
                    device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1f, 0);
                }

                tuple.Node.Render(info, this, tuple.Transform);
            }

            debugger.End();
        }

        private void QueueRenderObjects(ICollection<ISceneNode> nodes, IList<RenderNode> renderQueue, RenderInfo info, Matrix4x4 worldTransform)
        {
            foreach (var node in nodes)
            {
                if (!node.IsVisible)
                    continue;

                var transform = node.Transform * worldTransform;
                renderQueue.Add(new RenderNode { Priority = node.RenderPriority, Transform = transform, Node = node });
                QueueRenderObjects(node.Children, renderQueue, info, transform);
            }
        }

        private void RenderGrid(DebugDraw debugger, GraphicsDevice device)
        {
            debugger.Begin(Camera.View, Camera.Projection);
            int gridSize = 10;
            int gridHalfSize = gridSize / 2;
            for (int x = -gridHalfSize; x <= gridHalfSize; ++x)
            {
                for (int z = -gridHalfSize; z <= gridHalfSize; ++z)
                {
                    debugger.DrawLine(new Vector3(x, 0, -gridHalfSize), new Vector3(x, 0, gridHalfSize), new Color(0.2f, 0.2f, 0.2f));
                    debugger.DrawLine(new Vector3(-gridHalfSize, 0, z), new Vector3(gridHalfSize, 0, z), new Color(0.2f, 0.2f, 0.2f));
                }
            }
            debugger.End();

            device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1f, 0);

            debugger.Begin(Camera.View, Camera.Projection);
            debugger.DrawLine(Vector3.Zero, Vector3.UnitX * 10, Color.Red);
            debugger.DrawLine(Vector3.Zero, Vector3.UnitY * 10, Color.Lime);
            debugger.DrawLine(Vector3.Zero, Vector3.UnitZ * 10, Color.Blue);
            debugger.End();
        }
    }
}