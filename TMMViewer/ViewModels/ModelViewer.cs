using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TMMViewer.Data;
using TMMViewer.Data.Render;
using TMMViewer.Data.Render.Debug;
using TMMViewer.ViewModels.MonoGameControls;

namespace TMMViewer.ViewModels
{
    public partial class ModelViewer(MainWindowViewModel viewModel) : MonoGameViewModel
    {
        private Scene _scene => viewModel.Scene;
        private DebugDraw _debugger;

        public bool LockInput { get; set; } = false;

        public override void Initialize()
        {
            base.Initialize();
            _debugger = new DebugDraw(GraphicsDevice);
        }

        public override void OnMouseWheel(MouseStateArgs args, int delta)
        {
            if (LockInput)
                return;

            _scene.Camera.Update(args, delta);
        }


        public override void OnMouseMove(MouseStateArgs mouseState)
        {
            if (LockInput)
                return;

            _scene.Camera.Update(mouseState, 0);
        }

        public override void OnMouseDown(MouseStateArgs mouseState)
        {
            if (LockInput)
                return;

            _scene.Camera.Update(mouseState, 0);

        }

        public override void OnMouseUp(MouseStateArgs mouseState)
        {
            if (LockInput)
                return;

            _scene.Camera.Update(mouseState, 0);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            var renderInfo = new RenderInfo(gameTime, GraphicsDevice, _debugger);

            _scene.Render(renderInfo);
        }
    }
}
