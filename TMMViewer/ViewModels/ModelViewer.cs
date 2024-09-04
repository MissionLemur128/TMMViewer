using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TMMViewer.Data.Render;
using TMMViewer.ViewModels.MonoGameControls;

namespace TMMViewer.ViewModels
{
    public class ModelViewer : MonoGameViewModel
    {
        private Scene _scene;

        public ModelViewer(Scene scene)
        {
            _scene = scene;
        }

        public override void LoadContent()
        {
        }

        public override void OnMouseUp(MouseStateArgs mouseState)
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            _scene.Render(GraphicsDevice);
        }
    }
}
