using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TMVector4 = System.Numerics.Vector4;

namespace TMMViewer.Data.Render
{
    public class Bone
    {
        public static Effect? Material { get; set; }
        public static Model? Mesh { get; set; }

        public int index;
        public int parent;

        public List<Bone> Children { get; } = new List<Bone>();

        public TMVector4 unknown { get; set; } = new TMVector4(0, 0, 0, 1);
        public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;

        // unsure on this matrix, assumming based on https://community.khronos.org/t/applying-simplifying-the-skinning-in-a-dae/109720
        public Matrix4x4 GlobalTransform { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 InverseGlobalTransform { get; set; } = Matrix4x4.Identity;

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

        public void Render(Skeleton skeleton, Matrix4x4 world)
        {
            if (Material == null || Mesh == null)
            {
                throw new InvalidOperationException("Bone.Material and Bone.Mesh must be initialized before rendering.");
            }

            foreach (var mesh in Mesh.Meshes)
            {
                //var world = GetParentMatrix(skeleton);
                //var scale = GetScale(skeleton);
                Material.Parameters["_world"].SetValue(world);
                mesh.Draw();
            }
        }


        //private Matrix GetScale(Skeleton skeleton)
        //{
        //    //if ( skeleton.Bones.TryGetValue((ushort)index, out var bone) &&
        //    //    skeleton.Bones.TryGetValue((ushort)parent, out var boneParent))
        //    //{
        //    //    var v1 = bone.RestTransform.Translation;
        //    //    var v2 = boneParent.RestTransform.Translation;
        //    //    var scale = 1f / (v2 - v1).Length();
        //    //    return Matrix4x4.CreateScale(new System.Numerics.Vector3(scale, scale, scale));
        //    //}

        //    return Matrix4x4.Identity;
        //}

        //private Matrix GetParentMatrix(Skeleton skeleton)
        //{
        //    var world = RestTransform;
        //    var parentID = parent;
        //    List<Bone> boneChain = new();

        //    while (parentID >= 0 && skeleton.Bones.TryGetValue((ushort)parentID, out var bone))
        //    {
        //        var det = bone.RestTransform.GetDeterminant();
        //        //m2 = Matrix4x4.Transpose(m2);
        //        var neg = Negate(bone.RestTransform);
        //        var transinverse = Matrix4x4.Transpose(bone.RestTransform);


        //        var s = Matrix4x4.Invert(bone.InversePoseShape, out var inverse);

        //        if (!CompareMatrix(bone.PoseShape, inverse))
        //        {
        //            var a = 0;
        //        }
        //        boneChain.Add(bone);

        //        world = world * bone.RestTransform;// * transinverse * inverse;// * transinverse * inverse;// * transinverse;// bone.Transform * bone.TransposeOfInverseTransform * bone.InverseTransform;
        //        //return Matrix4x4.CreateTranslation(new System.Numerics.Vector4(bone.unknown[0], bone.unknown[1], bone.unknown[2], bone.unknown[3] * );// bone.tr;
        //        parentID = bone.parent;
        //    }   
        //    if (boneChain.Count > 5)
        //    {
        //        var a = skeleton.Bones.TryGetValue((ushort)parent, out var bone);
        //        var b = 0;
        //    }
        //    return world;
        //}

        //private Matrix4x4 Negate(Matrix4x4 transform)
        //{
        //    Matrix4x4 mat = Matrix4x4.Identity;
        //    for (int i = 0; i < 4; i++)
        //    {
        //        for (int j = 0; j < 4; j++)
        //        {
        //            mat[i, j] = i == j? transform[i, j] : -transform[i, j];
        //        }
        //    }
        //    return mat;
        //}

        //private bool CompareMatrix(Matrix4x4 transform, Matrix4x4 transinverse)
        //{
        //    for (int i = 0; i < 4; i++)
        //    {
        //        for (int j = 0; j < 4; j++)
        //        {
        //            if (Math.Abs(transform[i, j] - transinverse[i, j]) > 0.0001)
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}
    }

    public class Skeleton
    {
        public List<Bone> Bones { get; } = new();

        public void Render()
        {
            Bones.Where(b => b.parent == -1).ToList().ForEach(r => RenderBoneRecurse(r, Matrix4x4.Identity));
        }

        internal void AddBones(Bone bone3D)
        {
            var parent = Bones.FirstOrDefault(b => b.index == bone3D.parent);
            if (parent != null)
            {
                parent.Children.Add(bone3D);
            }
            Bones.Add(bone3D);
        }

        private void RenderBoneRecurse(Bone bone, Matrix4x4 world)
        {
            world = bone.LocalTransform * world;
            bone.Render(this, world);
            foreach (var child in bone.Children)
            {
                RenderBoneRecurse(child, world);
            }
        }
    }
}
