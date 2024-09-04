using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TMMViewer.Data.Render
{
    public class DirectionalLight
    {
        public Color Color { get; set; } = Color.White;
        public Vector3 Direction { get; set; } = new Vector3(0, 1, 1);

        public void ApplyToMaterial(Effect effect)
        {
            effect.Parameters["_sunLightColor"].SetValue(Color.ToVector3());
            effect.Parameters["_sunLightDirection"].SetValue(Direction);
        }
    }
}
