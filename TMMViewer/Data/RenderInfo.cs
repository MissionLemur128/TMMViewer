using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TMMViewer.Data.Render.Debug;
using System.Numerics;

namespace TMMViewer.Data
{
    public class RenderInfo(GameTime gameTime, GraphicsDevice graphicsDevice, DebugDraw debugDraw)
    {
        public GameTime GameTime => gameTime;
        public GraphicsDevice GraphicsDevice => graphicsDevice;
        public DebugDraw DebugDraw => debugDraw;
    }
}
