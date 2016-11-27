// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Timing;

namespace osu.Framework.Graphics3D
{
    /// <summary>
    /// 3D scene root drawable
    /// </summary>
    public class SceneRoot : Drawable3D
    {
        private List<Camera> cameras = new List<Camera>();

        /// <summary>
        /// The camera used to render all elements in this scene
        /// </summary>
        public Camera Camera { get; private set; }

        public override IFrameBasedClock Clock => CustomClock;

        protected override DrawNode3D CreateDrawNode() => new SceneRootDrawNode();

        public SceneRoot()
        {
            Scene = this;
        }

        /// <summary>
        /// Creates a draw node for the entire scene
        /// </summary>
        /// <returns></returns>
        public SceneRootDrawNode GenerateDrawNode()
        {
            SceneRootDrawNode n = (SceneRootDrawNode)CreateDrawNode();
            base.ApplyDrawNode(n);

            if(Camera == null)
                return null;
            
            n.ViewMatrix = Camera.ViewMatrix;
            n.InverseViewMatrix = Camera.InverseViewMatrix;
            n.ProjectionMatrix = Camera.ProjectionMatrix;
            n.ViewportSize = Camera.ViewportSize;

            foreach(var child in Children)
            {
                child.GenerateDrawNodes(n.Children);
            }

            n.Sort();

            return n;
        }

        internal void AddCamera(Camera camera)
        {
            cameras.Add(camera);
            UpdatePrimaryCamera();
        }

        internal void RemoveCamera(Camera camera)
        {
            cameras.Remove(camera);
            UpdatePrimaryCamera();
        }

        private void UpdatePrimaryCamera()
        {
            Camera = cameras.FirstOrDefault();
        }
    }
}