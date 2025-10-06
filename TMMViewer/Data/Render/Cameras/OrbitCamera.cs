using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TMMViewer.ViewModels.MonoGameControls;

namespace TMMViewer.Data.Render.Cameras
{
    public class OrbitCamera : Camera
    {
        public float Distance { get; set; } = 20;
        public float Yaw { get; set; } = 315;
        public float Pitch { get; set; } = 30;

        private Vector2 _lastCursorPosition;

        public OrbitCamera()
        {
            UpdatePosition();
        }

        public override void Update(MouseStateArgs mouseState, int delta)
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                var moveDistance = new Vector2(mouseState.Position.X, mouseState.Position.Y) - _lastCursorPosition;
                var rotateSensivity = 0.3f;
                Pitch += moveDistance.Y * rotateSensivity;
                Yaw += moveDistance.X * rotateSensivity;

                var pitchDegreeLimit = 89;
                Pitch = MathHelper.Clamp(Pitch, -pitchDegreeLimit, pitchDegreeLimit);
            }
            _lastCursorPosition = mouseState.Position;

            var zoomSensivity = -1f / 1000;
            Distance *= 1 + delta * zoomSensivity;
            Distance = MathHelper.Clamp(Distance, 0.1f, 1000);
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            var pitchRadian = MathHelper.ToRadians(Pitch);
            var yawRadian = MathHelper.ToRadians(Yaw);
            var cosPitch = MathF.Cos(pitchRadian);
            var dx = -MathF.Cos(yawRadian) * cosPitch;
            var dy = MathF.Sin(pitchRadian);
            var dz = -MathF.Sin(yawRadian) * cosPitch;

            Position = Target + Distance * new Vector3(dx, dy, dz);
        }
    }
}
