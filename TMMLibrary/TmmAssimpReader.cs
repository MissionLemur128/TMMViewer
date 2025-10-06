using Assimp;
using System.Numerics;
using TMMLibrary.IOUtils;
using TMMLibrary.TMMData.DataTypes;

namespace TMMLibrary
{
    /// <summary>
    /// Will eventually be able to fully convert any model to a TMM Model when the 
    /// file format is fully figured out. For now, only override existing TMM Models.
    /// </summary>
    public static class TmmAssimpReader
    {
        public static TmmModel ReadFromFile(TmmModel existingTmmModel, string inputPath)
        {
            if (existingTmmModel == null)
                throw new ArgumentException("Standard models can only modify already loaded Tmm files.");

            if (string.IsNullOrWhiteSpace(inputPath))
                throw new ArgumentException("Input path must not be empty.", nameof(inputPath));

            using var context = new AssimpContext();
            var scene = context.ImportFile(inputPath,  PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.MakeLeftHanded | PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);

            if (scene == null || scene.RootNode == null)
                throw new InvalidOperationException("Failed to load scene from file.");

            var tmmHeader = existingTmmModel.HeaderFile;
           // tmmHeader.Transform = Matrix4x4.Transpose(scene.RootNode.Transform);

            var tmmData = existingTmmModel.DataFile;
            var mesh = scene.Meshes.FirstOrDefault();
            if (mesh != null)
            {
                // Vertices
                tmmData.Vertices = [.. mesh.Vertices.Select((v, i) => new Vertex
                {
                    Origin = v,
                    Uv = ToUV(mesh.TextureCoordinateChannels[0][i]),
                    Normal = mesh.Normals[i],
                    Tangent = NormalEncoding.GetTangent( mesh.Normals[i], mesh.Tangents[i], mesh.BiTangents[i])
                })];
                tmmHeader.BoundingInfo.BoundingBoxes = new TMM.DataTypes.BoundingBox(tmmData.Vertices.Select(x => x.Origin));

                // Indices
                tmmData.Indices = [.. mesh.Faces.SelectMany(f => f.Indices).Reverse().Select(x => (ushort)x)];

                // Bone weights
                if (tmmHeader.Bones.Length > 0)
                {
                    var boneList = tmmHeader.Bones;
                    tmmData.BoneWeights = new BoneWeights[tmmData.Vertices.Length];

                    for (var i = 0; i < tmmData.BoneWeights.Length; ++i)
                    {
                        var boneWeight = new BoneWeights
                        {
                            BoneIndices = new byte[4],
                            Weights = new byte[4]
                        };

                        var weightsForVerex = NewMethod(mesh, boneList, i)
                            .OrderByDescending(x => x.weight)
                            .ToArray();

                        var maxBoneWeight = 4;
                        for (int j = 0; j < maxBoneWeight && j < weightsForVerex.Length; ++j)
                        {
                            var weight = weightsForVerex[j];
                            var reversedJ = boneWeight.WeightsCount - j - 1;
                            boneWeight.BoneIndices[reversedJ] = weight.boneIndex;
                            boneWeight.Weights[reversedJ] = weight.weight;
                        }

                        tmmData.BoneWeights[i] = boneWeight;
                    }
                }

                tmmData.HeightGradient = new byte[tmmData.Vertices.Length * 2];
                for (int i = 0; i < tmmData.Vertices.Length; ++i)
                {
                    var gradient = mesh.VertexColorChannelCount > 0
                        ? mesh.VertexColorChannels[0][i].X
                        : (tmmData.Vertices[i].Origin.Y / 50); // TODO: Simple calculation but not the same as what AoMR uses

                    var gradientIndex = i * 2;
                    var scaledGradient = (ushort)(ushort.MaxValue * gradient);
                    tmmData.HeightGradient[gradientIndex] = (byte)(scaledGradient & 0xFF);
                    tmmData.HeightGradient[gradientIndex + 1] = (byte)((scaledGradient >> 8) & 0xFF);
                }

                // Update offsets and counts
                tmmHeader.MaterialVertexGroups[0].VertexCount = tmmData.Vertices.Length;
                tmmHeader.MaterialVertexGroups[0].IndexCount = tmmData.Indices.Length;
                tmmHeader.MaterialVertexGroups[0].IndexOffset = 0;
                tmmHeader.MaterialVertexGroups[0].VertexOffset = 0;                
            }

            existingTmmModel.Update();
            return existingTmmModel;
        }


        private static IEnumerable<(byte boneIndex, byte weight)> NewMethod(Mesh mesh, TMMLibrary.TMM.DataTypes.Bone[] boneList, int i)
        {
            foreach (var assimpBone in mesh.Bones)
            {
                var vertexWeight = assimpBone.VertexWeights.FindIndex(vw => vw.VertexID == i);
                if (vertexWeight < 0)
                    continue;

                var boneIndex = (byte)Array.FindIndex(boneList, b => b.Name == assimpBone.Name);
                var weight = (byte)(assimpBone.VertexWeights[vertexWeight].Weight * 255);
                yield return (boneIndex, weight);
            }
        }

        private static Vector2 ToUV(Vector3 vector3)
        {
            return new Vector2(vector3.X, vector3.Y);
        }
    }
}
