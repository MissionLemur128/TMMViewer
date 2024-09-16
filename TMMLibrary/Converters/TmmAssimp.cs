using TMMLibrary.TMM;
using Assimp;
using AssimpBone = Assimp.Bone;

namespace TMMLibrary.Converters
{
    public static class TmmAssimp
    {
        public static bool WriteToFile(TmmFile file, TmmDataFile model, string outputPath, string format)
        {
            var scene = new Scene();
            scene.Materials.Add(new Material());
            
            var root = new Node("RootNode");
            scene.RootNode = root;
            root.Transform = Assimp.Matrix4x4.Identity;

            var armatureNode = new Node(Path.GetFileNameWithoutExtension(outputPath));
            armatureNode.Transform = Assimp.Matrix4x4.Identity;
            root.Children.Add(armatureNode);

            var meshNode = new Node("mesh");
            meshNode.Transform = Assimp.Matrix4x4.Identity;
            root.Children.Add(meshNode);

            var boneNodes = new List<Node>();

            for (int i = 0; i < file.ModelInfos.Length; ++i)
            {
                var modelInfo = file.ModelInfos[i];
                for (int j = 0; j < modelInfo.Bones.Count(); ++j)
                {
                    var bone = modelInfo.Bones[j];
                    var node = new Node(bone.Name);

                    var transform =  bone.Transform;
                    node.Transform = ConvertMatrix4x4(transform);
                    node.Metadata.Add("UserProperties", new Metadata.Entry(MetaDataType.String, string.Empty));
                    node.Metadata.Add("IsNull", new Metadata.Entry(MetaDataType.Bool, true));
                    node.Metadata.Add("DefaultAttributeIndex", new Metadata.Entry(MetaDataType.Int32, 0));
                    node.Metadata.Add("InheritType", new Metadata.Entry(MetaDataType.Int32, 1));

                    boneNodes.Add(node);

                    if (bone.BoneParent == -1)
                        armatureNode.Children.Add(node);
                    else
                        boneNodes[bone.BoneParent].Children.Add(node);

                }
            }

            Mesh mesh = new Mesh(PrimitiveType.Triangle);

            for (int i = 0; i < model.Vertices.Length; ++i)
            {
                mesh.Vertices.Add(new Vector3D(model.Vertices[i].Origin.X, model.Vertices[i].Origin.Y, model.Vertices[i].Origin.Z));
                mesh.TextureCoordinateChannels[0].Add(new Vector3D(model.Vertices[i].Uv.X, model.Vertices[i].Uv.Y, 0));
                mesh.Normals.Add(new Vector3D(0, 0, 0));
            }

            for (int i = 0; i < file.ModelInfos.Length; ++i)
            {
                var modelInfo = file.ModelInfos[i];
                for (int j = 0; j < modelInfo.Bones.Count(); ++j)
                {
                    var bone = modelInfo.Bones[j];
                    var assimpBone = new AssimpBone();
                    assimpBone.OffsetMatrix = ConvertMatrix4x4(bone.Transform);
                    assimpBone.Name = bone.Name;

                    for (int k = 0; k < model.BoneWeights.Length; ++k)
                    {
                        for (int h = 0; h < 4; ++h)
                        {
                            if (model.BoneWeights[k].BoneIndices[h] == j && model.BoneWeights[k].Weights[h] > 0)
                            {
                                assimpBone.VertexWeights.Add(new VertexWeight(k, model.BoneWeights[k].Weights[h] / 255f));
                            }
                        }
                    }
                    mesh.Bones.Add(assimpBone);
                }
            }

            for (int i = 0; i < model.Indices.Length; i += 3)
            {
                var indices = new int[] { model.Indices[i + 2], model.Indices[i + 1], model.Indices[i] };
                mesh.Faces.Add(new Face(indices));
                var normal = Vector3D.Cross(mesh.Vertices[indices[1]] - mesh.Vertices[indices[0]], mesh.Vertices[indices[2]] - mesh.Vertices[indices[0]]);
                normal.Normalize();
                for (int j = 0; j < 3; ++j)
                {
                    mesh.Normals[indices[j]] += normal;
                }
            }

            for (int i = 0; i < mesh.Normals.Count; ++i)
            {
                mesh.Normals[i].Normalize();
            }
            mesh.MaterialIndex = 0;

            scene.Meshes.Add(mesh);
            meshNode.MeshIndices.Add(0);

            // Values copied from Blender FBX exporter
            scene.Metadata.Add("UpAxis", new Metadata.Entry(MetaDataType.Int32, 1));
            scene.Metadata.Add("UpAxisSign", new Metadata.Entry(MetaDataType.Int32, 1));
            scene.Metadata.Add("FrontAxis", new Metadata.Entry(MetaDataType.Int32, 2));
            scene.Metadata.Add("FrontAxisSign", new Metadata.Entry(MetaDataType.Int32, 1));
            scene.Metadata.Add("CoordAxis", new Metadata.Entry(MetaDataType.Int32, 0));
            scene.Metadata.Add("CoordAxisSign", new Metadata.Entry(MetaDataType.Int32, 1));
            scene.Metadata.Add("OriginalUpAxis", new Metadata.Entry(MetaDataType.Int32, -1));
            scene.Metadata.Add("OriginalUpAxisSign", new Metadata.Entry(MetaDataType.Int32, 1));
            scene.Metadata.Add("UnitScaleFactor", new Metadata.Entry(MetaDataType.Int32, 1));
            scene.Metadata.Add("OriginalUnitScaleFactor", new Metadata.Entry(MetaDataType.Int32, 1));
            scene.Metadata.Add("AmbientColor", new Metadata.Entry(MetaDataType.Vector3D,  new Vector3D(0, 0, 0)));

            scene.Metadata.Add("FrameRate", new Metadata.Entry(MetaDataType.Int32, 11));
            scene.Metadata.Add("TimeSpanStart", new Metadata.Entry(MetaDataType.Int32, 0));
            scene.Metadata.Add("TimeSpanStop", new Metadata.Entry(MetaDataType.Int32, 0));
            scene.Metadata.Add("CustomFrameRate", new Metadata.Entry(MetaDataType.Int32, 24));

            var context = new AssimpContext();
            var result = context.ExportFile(scene, outputPath, format, PostProcessSteps.MakeLeftHanded | PostProcessSteps.FlipUVs);

            return result;
        }


        private static Assimp.Matrix4x4 ConvertMatrix4x4(System.Numerics.Matrix4x4 t)
        {
            return new Assimp.Matrix4x4(
                t.M11, t.M21, t.M31, t.M41,
                t.M12, t.M22, t.M32, t.M42,
                t.M13, t.M23, t.M33, t.M43,
                t.M14, t.M24, t.M34, t.M44
            );
        }
    }
}
