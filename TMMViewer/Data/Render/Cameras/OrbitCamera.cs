using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TMMViewer.ViewModels.MonoGameControls;

namespace TMMViewer.Data.Render.Cameras
{
    public class OrbitCamera : Camera
    {
        public float Distance { get; set; } = 20;
        public float Yaw { get; set; } = -20;
        public float Pitch { get; set; } = 1;

        private Vector2 _lastCursorPosition;

        public OrbitCamera()
        {
            UpdatePosition();
        }

        public override void Update(MouseStateArgs mouseState, int delta)
        {
            var rotateSensivity = 0.3f;
            var zoomSensivity = -1f / 120;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                var moveDistance = new Vector2(mouseState.Position.X, mouseState.Position.Y) - _lastCursorPosition;
                Pitch += moveDistance.Y * rotateSensivity;
                Yaw += moveDistance.X * rotateSensivity;
            }

            _lastCursorPosition = mouseState.Position;
            Distance += delta * zoomSensivity;
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Position = Distance * new Vector3(
                            MathF.Cos(MathHelper.ToRadians(Yaw)) * MathF.Cos(MathHelper.ToRadians(Pitch)),
                            MathF.Sin(MathHelper.ToRadians(Pitch)),
                            MathF.Sin(MathHelper.ToRadians(Yaw)) * MathF.Cos(MathHelper.ToRadians(Pitch)));
        }
    }
}
