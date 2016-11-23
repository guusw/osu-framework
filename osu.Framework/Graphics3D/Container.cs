// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;

namespace osu.Framework.Graphics3D
{
    using BufferedContainer2D = Graphics.Containers.BufferedContainer;

    /// <summary>
    /// A drawable that contains objects rendered in 3D space
    /// </summary>
    public class Container : BufferedContainer2D
    {
        /// <summary>
        /// 3D content
        /// </summary>
        public new SceneRoot Content { get; } = new SceneRoot();

        /// <summary>
        /// The camera used to render all elements in this view
        /// </summary>
        public Camera Camera => Content.Camera;

        /// <summary>
        /// The 3D scene's root elements
        /// </summary>
        public new IEnumerable<Drawable> Children
        {
            get { return Content.Children; }
            set { Add(value); }
        }

        protected override osu.Framework.Graphics.DrawNode CreateDrawNode() => new ContainerDrawNode();

        protected internal override bool UpdateSubTree()
        {
            Content.UpdateSubTree();
            return base.UpdateSubTree();
        }

        protected override void UpdateLayout()
        {
            base.UpdateLayout();
            Camera.ViewportSize = DrawSize;
        }

        protected override void ApplyDrawNode(osu.Framework.Graphics.DrawNode node)
        {
            base.ApplyDrawNode(node);
            var n = (ContainerDrawNode)node;
            n.Scene = Content.GenerateDrawNode();
        }

        public void Add(IEnumerable<Drawable> collection)
        {
            Content.Add(collection);
        }

        public void Add(Drawable drawable)
        {
            Content.Add(drawable);
        }

        public void Remove(Drawable drawable)
        {
            Content.Add(drawable);
        }
    }
}