using TMMLibrary.TMM;
using Assimp;

namespace TMMLibrary.Converters
{
    public static class TmmAssimp
    {
        public static bool WriteToFile(TmmFile file, TmmDataFile model, string outputPath, string format)
        {
            Scene scene = new Scene();
            var root = new Node("root");
            scene.RootNode = root;
            scene.Materials.Add(new Material());

            Mesh mesh = new Mesh(PrimitiveType.Triangle);

            for (int i = 0; i < model.Vertices.Length; ++i)
            {
                mesh.Vertices.Add(new Vector3D(-model.Vertices[i].Origin.X, model.Vertices[i].Origin.Y, model.Vertices[i].Origin.Z));
                mesh.TextureCoordinateChannels[0].Add(new Vector3D(model.Vertices[i].Uv.X, 1 - model.Vertices[i].Uv.Y, 0));
                mesh.Normals.Add(new Vector3D(0, 0, 0));
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
            root.MeshIndices.Add(0);

            var context = new AssimpContext();
            return context.ExportFile(scene, outputPath, format, PostProcessSteps.None);  
        }
    }
}
