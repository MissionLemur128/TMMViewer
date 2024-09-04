using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TMMViewer.Data.Render
{
    public class Environment
    {
        public Color AmbientColor { get; set; } = Color.Gray;

        public void ApplyToMaterial(Effect effect)
        {
            effect.Parameters["_ambientLightColor"].SetValue(AmbientColor.ToVector3());
        }
    }
}
