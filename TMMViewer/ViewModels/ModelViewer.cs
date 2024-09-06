using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TMMViewer.Data.Render;
using TMMViewer.ViewModels.MonoGameControls;
using TMMViewer.Data;

namespace TMMViewer.ViewModels
{
    public class ModelViewer : MonoGameViewModel
    {
        private Scene _scene;

        public ModelViewer(Scene scene)
        {
            _scene = scene;
        }

        public override void OnMouseWheel(MouseStateArgs args, int delta)
        {
            _scene.Camera.Update(args, delta);
        }


        public override void OnMouseMove(MouseStateArgs mouseState)
        {
            _scene.Camera.Update(mouseState, 0);
        }

        public override void OnMouseDown(MouseStateArgs mouseState)
        {
            _scene.Camera.Update(mouseState, 0);
        }

        public override void OnMouseUp(MouseStateArgs mouseState)
        {
            _scene.Camera.Update(mouseState, 0);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            _scene.Render(GraphicsDevice);
        }
    }
}
