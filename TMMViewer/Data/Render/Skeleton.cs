using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMMViewer.Data.Render
{
    public class Bone
    {
        public static Effect? Material { get; set; }
        public static Model? Mesh { get; set; }

        public int index;
        public int parent;

        public byte[] data { get; set; } = [];

        public Matrix Transform
        {
            get {
                var v = new Vector4[4];
                var offset = 4 * 4;
                for (int i = 0; i < 4; i++)
                {
                    v[i] = new Vector4(
                        BitConverter.ToSingle(data, offset + i * 16),
                        BitConverter.ToSingle(data, offset + i * 16 + 4),
                        BitConverter.ToSingle(data, offset + i * 16 + 8),
                        BitConverter.ToSingle(data, offset + i * 16 + 12));
                }

                var mat = new Matrix(v[0], v[1], v[2], v[3]);
                //mat = Matrix.Transpose(Matrix.Invert(mat));
                return mat;
                //return Matrix.CreateWorld(v[0], v[1], v[2]);
            }
        }

        public static void InitGlobal(GraphicsDevice d, ContentManager c)
        {
            Material = c.Load<Effect>("Effects/BoneShader");
            Mesh = c.Load<Model>("Meshes/bone");

            foreach (var mesh in Mesh.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = Material;
                }
            }
        }

        public void Render(Skeleton skeleton)
        {
            if (Material == null || Mesh == null)
            {
                throw new InvalidOperationException("Bone.Material and Bone.Mesh must be initialized before rendering.");
            }

            foreach (var mesh in Mesh.Meshes)
            {
                Material.Parameters["_world"].SetValue(GetParentMatrix(skeleton));
                mesh.Draw();
            }
        }

        private Matrix GetParentMatrix(Skeleton skeleton)
        {
            var world = Transform;
            var parentID = parent;
            while (parentID >= 0 && skeleton.Bones.TryGetValue((ushort)parentID, out var bone))
            {
                world = world * bone.Transform ;
                parentID = bone.parent;
            }   
            return world;
        }
    }

    public class Skeleton
    {
        public Dictionary<ushort, Bone> Bones { get; } = new ();

        public void Render()
        {
            foreach (var bone in Bones.Values)
            {
                bone.Render(this);
            }
        }
    }
}
