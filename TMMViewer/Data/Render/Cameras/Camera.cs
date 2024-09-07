using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TMMViewer.ViewModels.MonoGameControls;

namespace TMMViewer.Data.Render.Cameras
{
    public class Camera
    {
        public Vector3 Position { get; set; } = new Vector3(12, 10, 12);
        public Vector3 Target { get; set; } = new Vector3(0, 5, 0);
        public Vector3 Up { get; set; } = new Vector3(0, 1, 0);
        public float FieldOfView { get; set; } = MathHelper.PiOver4;
        public float AspectRatio { get; set; } = 1;
        public float NearPlaneDistance { get; set; } = 0.1f;
        public float FarPlaneDistance { get; set; } = 1000;

        public Matrix View => Matrix.CreateLookAt(Position, Target, Up);
        public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlaneDistance, FarPlaneDistance);

        public Color BackgroundColor { get; set; } = Color.Black;

        public virtual void Update(MouseStateArgs mouseState, int delta)
        {

        }


        public void ApplyToMaterial(Effect effect)
        {
            effect.Parameters["_view"].SetValue(View);
            effect.Parameters["_projection"].SetValue(Projection);
        }
    }
}
