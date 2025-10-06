using Microsoft.Xna.Framework;
using TMMLibrary;
using TMMViewer.Data.Render.Cameras;
using TMMViewer.Data.Render.Nodes;
using TMMViewer.ViewModels.MonoGameControls;

namespace TMMViewer.Data.Render
{
    public partial class Scene
    {
        public static Scene CreateFromTmmModel(TmmModel file, IMonoGameViewModel monoGame)
        {
            return new Scene(file, monoGame);
        }

        private Scene(TmmModel file, IMonoGameViewModel _monoGame)
        {
            var headerFile = file.HeaderFile;
            var dataFile = file.DataFile;

            var graphicsDevice = _monoGame.GraphicsDeviceService.GraphicsDevice;
            var _content = _monoGame.Content;

            var bounding = headerFile.BoundingInfo;
            Children.Add(new BoundingVolume(bounding) { IsVisible = false });

            var cameraTarget = Vector3.Zero;

            var mesh = new GenericNode() { Name = "Mesh" };
            for (int m = 0; m < headerFile.MaterialVertexGroups.Length; ++m)
            {
                var material = headerFile.MaterialVertexGroups[m];
                var meshObject = new Render.Nodes.Mesh(graphicsDevice, _content, file, material);
                cameraTarget += (bounding.BoundingBoxes.Max + bounding.BoundingBoxes.Min) / 2;
                mesh.Children.Add(meshObject);
            }
            Children.Add(mesh);

            if (headerFile.Bones.Length > 0)
            {
                var skeleton = new GenericNode() { Name = "Skeleton" };
                var bones = headerFile.Bones.Select(
                    bone => new Bone(_content, bone)).ToList();

                foreach (var bone in bones)
                {
                    if (bone.BoneData.BoneParentIndex != -1)
                    {
                        bones[bone.BoneData.BoneParentIndex].Children.Add(bone);
                    }
                    else
                    {
                        skeleton.Children.Add(bone);
                    }
                }

                foreach (var bone in headerFile.AttachPoints)
                {
                    var attachPoint = new AttachPoint(bone);

                    var parent = attachPoint.Data.BoneParent;
                    if (parent >= 0 && parent < bones.Count)
                    {
                        bones[parent].Children.Add(attachPoint);
                    }
                    else
                    {
                        skeleton.Children.Add(attachPoint);
                    }
                }
                Children.Add(skeleton);
            }

            Camera = new OrbitCamera
            {
                Target = cameraTarget / headerFile.MaterialVertexGroups.Length,
                Distance = 4 * headerFile.BoundingInfo.BoundingRadius
            };
        }
    }
}
