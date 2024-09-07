﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TMMViewer.Data.Render.Cameras;

namespace TMMViewer.Data.Render
{
    public class Scene
    {
        public Environment Environment { get; set; } = new();
        public Camera Camera { get; set; } = new OrbitCamera();
        public DirectionalLight DirectionalLight { get; set; } = new();
        public List<Mesh> Meshes { get; set; } = new();
        public Skeleton Skeleton { get; set; } = new();
        public RenderMode RenderMode { get; set; } = RenderMode.Solid;

        public void Render(GraphicsDevice device)
        {
            Camera.AspectRatio = device.Viewport.AspectRatio;
            device.Clear(Camera.BackgroundColor);

            foreach (var mesh in Meshes)
            {
                Camera.ApplyToMaterial(mesh.Material);
                Environment.ApplyToMaterial(mesh.Material);
                DirectionalLight.ApplyToMaterial(mesh.Material);
                mesh.Render(RenderMode);
            }

            if (RenderMode == RenderMode.Bones || RenderMode == RenderMode.BoneWeights)
            {
                device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1f, 0);
                Camera.ApplyToMaterial(Bone.Material);
                Skeleton.Render();
            }
        }
    }
}
