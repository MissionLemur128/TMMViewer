using Color = Microsoft.Xna.Framework.Color;


namespace TMMViewer.Data.Render.Nodes
{
    public class ColorCollection
    {
        private static readonly Dictionary<int, Color> _colors = [];

        public static Color GetColorForIndex(int index)
        {
            if (!_colors.TryGetValue(index, out Color value))
            {
                value = new Color(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
                _colors.Add(index, value);
            }
            return value;
        }
    }
}
