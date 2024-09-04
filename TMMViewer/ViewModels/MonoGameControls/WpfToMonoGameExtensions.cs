using Microsoft.Xna.Framework;

namespace TMMViewer.ViewModels.MonoGameControls;

public static class WpfToMonoGameExtensions
{
    public static Vector2 ToVector2(this System.Windows.Point point) => new Vector2((float)point.X, (float)point.Y);
}