using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using TMVector4 = System.Numerics.Vector4;

namespace TMMViewer.Data.Render.Nodes
{
    public class Bone : ISceneNode
    {
        public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;

        public Matrix4x4 GlobalTransform { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 InverseGlobalTransform { get; set; } = Matrix4x4.Identity;

        public override Matrix4x4 Transform { get => LocalTransform; set => LocalTransform = value; }
        public override string Name { get; set; }

        private static Effect? _material;
        private static Model? _mesh;
        private static bool _hasInitialized = false;

        public TMMLibrary.TMM.DataTypes.Bone BoneData { get; }

        public Bone(ContentManager c, TMMLibrary.TMM.DataTypes.Bone bone)
        {
            RenderPriority = (int)RenderPriorityDefinitions.Foreground;

            BoneData = bone;

            Name = bone.Name;
            LocalTransform = bone.LocalTransform;
            GlobalTransform = bone.GlobalTransform;
            InverseGlobalTransform = bone.InverseGlobalTransform;
                
            if (_hasInitialized)
                return;
            
            _hasInitialized = true;
            _material = c.Load<Effect>("Effects/BoneShader");
            _mesh = c.Load<Model>("Meshes/bone");

            foreach (var mesh in _mesh.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = _material;
                }
            }
        }

        public override void Render(RenderInfo info, Scene root, Matrix4x4 transform)
        {
            if (root.RenderMode == RenderMode.BoneWeights)
            { 
                root.Camera.ApplyToMaterial(_material);
                var scale = Matrix4x4.CreateScale(0.5f);
                foreach (var mesh in _mesh.Meshes)
                {
                    var color = IsSelected ? Color.Yellow : Color.Green;
                    _material.Parameters["_color"].SetValue(color.ToVector3());
                    _material.Parameters["_world"].SetValue(scale * transform);
                    mesh.Draw();
                }
            }
        }
    }
}
