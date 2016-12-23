// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Timing;

namespace osu.Framework.Graphics3D
{
    /// <summary>
    /// A drawable that contains objects rendered in 3D space
    /// </summary>
    public class Render3DContainer : BufferedContainer
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
        public new IEnumerable<Drawable3D> Children
        {
            get { return Content.Children; }
            set { Add(value); }
        }

        protected override osu.Framework.Graphics.DrawNode CreateDrawNode() => new Render3DContainerDrawNode();

        internal override void UpdateClock(IFrameBasedClock clock)
        {
            Content.Clock = clock;
            base.UpdateClock(clock);
        }

        public void Add(IEnumerable<Drawable3D> collection)
        {
            Content.Add(collection);
        }

        public void Add(Drawable3D drawable)
        {
            Content.Add(drawable);
        }

        public void Remove(Drawable3D drawable)
        {
            Content.Add(drawable);
        }

        protected internal override bool UpdateSubTree()
        {
            if(!base.UpdateSubTree())
                return false;

            // Update viewport size on camera
            if(Camera != null)
                Camera.ViewportSize = DrawSize;

            Content.UpdateSubTree();
            return base.UpdateSubTree();
        }

        protected override void ApplyDrawNode(osu.Framework.Graphics.DrawNode node)
        {
            base.ApplyDrawNode(node);
            var n = (Render3DContainerDrawNode)node;
            n.Scene = Content.GenerateDrawNode();
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void Load(BaseGame game)
        {
            Content.PerformLoad(game);
        }
    }
}