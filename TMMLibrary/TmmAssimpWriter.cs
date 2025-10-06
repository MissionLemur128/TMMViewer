using Assimp;
using System.Numerics;
using TMMLibrary.TMM;
using TMMLibrary.TMM.DataTypes;
using TMMLibrary.TMMData;

namespace TMMLibrary
{
    /// <summary>
    /// Notes: 
    /// - Assimp requries all transforms to be transposed.
    /// - Assimp has several bugs with exporting. Exporting multiple materials to FBX will create a poor mesh if it has bone weights.
    /// </summary>
    public static class TmmAssimpWriter
    {
        private const int NoParentIndex = -1;
        private const int TextureDimension = 2; // UV has 2 components (X, Y)

        public static bool WriteToFile(TmmModel file, string outputPath)
        {
            ArgumentNullException.ThrowIfNull(file);
            if (string.IsNullOrWhiteSpace(outputPath)) throw new ArgumentException("Output path must not be empty.", nameof(outputPath));

            var extension = Path.GetExtension(outputPath);
            string format = SupportedFiles.GetAssimpFormat(extension);

            var tmmHeader = file.HeaderFile;
            var tmmData = file.DataFile;

            var root = new Assimp.Node("RootNode");

            var scene = new Assimp.Scene
            {
                RootNode = root,
                SceneFlags = Assimp.SceneFlags.NonVerboseFormat
            };

            CreateMaterialNodes(tmmHeader, scene);

            if (IsModelAUnit(file))
            {
                WriteSkinnedMeshNodes(tmmHeader, tmmData, root, scene);
            }
            else
            {
                CreateSingleMeshNode(tmmHeader, tmmData, scene);
            }

            using var context = new AssimpContext();
            return context.ExportFile(scene, outputPath, format, PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs);
        }

        private static bool IsModelAUnit(TmmModel file)
        {
            return file.HeaderFile.Bones.Length > 0;
        }

        private static void WriteSkinnedMeshNodes(TmmFile tmmHeader, TmmDataFile tmmData, Node root, Scene scene)
        {
            var armatureNode = CreateArmatureNode(tmmHeader.Bones, out var boneNodes);
            AddAttachPoints(tmmHeader.AttachPoints, boneNodes, armatureNode);
            root.Children.Add(armatureNode);

            var meshNode = CreateMeshNode(tmmHeader, tmmData, scene);
            armatureNode.Children.AddRange([.. meshNode]);
        }

        private static void CreateSingleMeshNode(TmmFile tmmHeader, TmmDataFile tmmData, Assimp.Scene scene)
        {
            var meshNode = new Assimp.Node("Mesh")
            {
                Transform = FBXIdentityMatrix()
            };
            scene.RootNode.Children.Add(meshNode);

            for (int i = 0; i < tmmHeader.MaterialVertexGroups.Length; ++i)
            {
                var materialGroup = tmmHeader.MaterialVertexGroups[i];
                var materialName = tmmHeader.Materials[materialGroup.MaterialIndex];

                var mesh = new Assimp.Mesh(Assimp.PrimitiveType.Triangle)
                {
                    UVComponentCount = { [0] = TextureDimension },
                    MaterialIndex = i,
                    Name = $"Mesh_{materialName}"
                };
                       
                AddVerticesAndFaces(mesh, tmmData, materialGroup);
                AddBoneWeights(mesh, tmmHeader.Bones, tmmData, materialGroup);

                meshNode.MeshIndices.Add(scene.Meshes.Count);
                scene.Meshes.Add(mesh);
            }
        }

        private static Assimp.Node CreateArmatureNode(TMM.DataTypes.Bone[] bones, out List<Assimp.Node> boneNodes)
        {
            var armatureNode = new Assimp.Node("Bones") 
            { 
                Transform = FBXIdentityMatrix()
            };
            boneNodes = new List<Assimp.Node>(bones.Length);

            for (int j = 0; j < bones.Length; ++j)
            {
                var bone = bones[j];
                var node = new Assimp.Node(bone.Name)
                {
                    Transform = Matrix4x4.Transpose(bone.LocalTransform),
                };
                boneNodes.Add(node);

                if (bone.BoneParentIndex == NoParentIndex)
                    armatureNode.Children.Add(node);
                else
                    boneNodes[bone.BoneParentIndex].Children.Add(node);
            }
            return armatureNode;
        }

        private static void AddAttachPoints(AttachPoint[] attachPoints, List<Assimp.Node> boneNodes, Assimp.Node armatureNode)
        {
            for (int i = 0; i < attachPoints.Length; ++i)
            {
                var attachPoint = attachPoints[i];
                var attachNode = new Assimp.Node(attachPoint.Name1)
                {
                    Transform = Matrix4x4.Transpose(attachPoint.LocalTransform)
                };

                if (attachPoint.BoneParent >= 0 && attachPoint.BoneParent < boneNodes.Count)
                    boneNodes[attachPoint.BoneParent].Children.Add(attachNode);
                else
                    armatureNode.Children.Add(attachNode);
            }
        }

        private static void CreateMaterialNodes(TmmFile tmmHeader, Assimp.Scene scene)
        {
            for (int i = 0; i < tmmHeader.MaterialVertexGroups.Length; ++i)
            {
                var materialGroup = tmmHeader.MaterialVertexGroups[i];
                var materialName = tmmHeader.Materials[materialGroup.MaterialIndex];
                var material = new Assimp.Material { Name = tmmHeader.Materials[materialGroup.MaterialIndex] };
                scene.Materials.Add(material);
            }
        }

        private static IEnumerable<Assimp.Node> CreateMeshNode(TmmFile tmmHeader, TmmDataFile tmmData, Assimp.Scene scene)
        {
            for (int i = 0; i < tmmHeader.MaterialVertexGroups.Length; ++i)
            {
                var materialGroup = tmmHeader.MaterialVertexGroups[i];
                var materialName = tmmHeader.Materials[materialGroup.MaterialIndex];
                var mesh = new Assimp.Mesh(Assimp.PrimitiveType.Triangle)
                {
                    UVComponentCount = { [0] = TextureDimension },
                    MaterialIndex = i,
                    Name = $"Mesh_{materialName}"
                };

                AddVerticesAndFaces(mesh, tmmData, materialGroup);
                AddBoneWeights(mesh, tmmHeader.Bones, tmmData, materialGroup);

                var meshNode = new Assimp.Node(materialName)
                {
                    Transform = Matrix4x4.Identity
                };
                meshNode.MeshIndices.Add(scene.Meshes.Count);
                scene.Meshes.Add(mesh);
                yield return meshNode;
            }
        }

        private static void AddVerticesAndFaces(Assimp.Mesh mesh, TmmDataFile tmmData, MaterialVertexGroup materialGroup)
        {
            var gradientMask = new List<Vector4>();
            var vertexStart = materialGroup.VertexOffset;
            var vertexEnd = vertexStart + materialGroup.VertexCount;
            for (var j = vertexStart; j < vertexEnd; ++j)
            {
                var vertex = tmmData.Vertices[j];
                mesh.Vertices.Add(vertex.Origin);
                mesh.TextureCoordinateChannels[0].Add(new Vector3(vertex.Uv.X, vertex.Uv.Y, 0));
                mesh.Normals.Add(vertex.Normal);
                //mesh.Tangents.Add(vertex.Tangent);
                //mesh.Tangents.Add(vertex.Tangent);

                var gradientIndex = j;
                gradientMask.Add(new Vector4(tmmData.HeightGradient[gradientIndex] / 255f, tmmData.HeightGradient[gradientIndex + 1] / 255f, 0, 0));
            }
            mesh.VertexColorChannels[0] = gradientMask;
          
            var indexStart = materialGroup.IndexOffset;
            var indexEnd = indexStart + materialGroup.IndexCount;
            for (var j = indexStart; j < indexEnd; j += 3)
            {
                var indices = new int[]
                {
                    tmmData.Indices[j + 2],
                    tmmData.Indices[j + 1],
                    tmmData.Indices[j]
                };
                mesh.Faces.Add(new Assimp.Face(indices));
            }
        }

        private static void AddBoneWeights(Assimp.Mesh mesh, TMM.DataTypes.Bone[] bones, TmmDataFile tmmData, MaterialVertexGroup materialGroup)
        {
            var vertexStart = materialGroup.VertexOffset;
            var vertexEnd = vertexStart + materialGroup.VertexCount;

            for (int boneIndex = 0; boneIndex < bones.Length; ++boneIndex)
            {
                var bone = bones[boneIndex];
                var assimpBone = new Assimp.Bone
                {
                    Name = bone.Name,
                    OffsetMatrix = Matrix4x4.Transpose(bone.LocalTransform),
                };

                for (int k = vertexStart; k < vertexEnd; ++k)
                {
                    var index = k - vertexStart;
                    var boneWeight = tmmData.BoneWeights[k];

                    for (int h = 0; h < boneWeight.WeightsCount; ++h)
                    {
                        var weight = boneWeight.Weights[h] / 255f;
                        if (boneWeight.BoneIndices[h] == boneIndex && weight > 0)
                        {
                            assimpBone.VertexWeights.Add(new Assimp.VertexWeight(index, weight));
                        }
                    }
                }
                mesh.Bones.Add(assimpBone);
            }
        }

        private static Matrix4x4 FBXIdentityMatrix()
        {
            // Scaling factor for FBX export, Assimp uses 100 as a default scaling factor.
            return new Matrix4x4(
                100, 0, 0, 0,
                0, 100, 0, 0,
                0, 0, 100, 0,
                0, 0, 0, 1);
        }
    }
}
