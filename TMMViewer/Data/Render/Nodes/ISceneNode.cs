using System.Collections.ObjectModel;
using System.Numerics;

namespace TMMViewer.Data.Render.Nodes
{
    public abstract class ISceneNode
    {
        public bool IsVisible { get; set; } = true;

        public abstract string Name { get; set; }

        public virtual int RenderPriority { get; set; } = (int)RenderPriorityDefinitions.Default;

        public virtual bool IsExpanded { get; set; } = true;

        public virtual bool IsSelected { get; set; }

        public virtual Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

        public ObservableCollection<ISceneNode> Children { get; } = new ObservableCollection<ISceneNode>();

        public abstract void Render(RenderInfo info, Scene root, Matrix4x4 worldTransform);
    }
}
