// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

namespace osu.Framework.Graphics3D
{
    /// <summary>
    /// 3D scene root drawable
    /// </summary>
    public class SceneRoot : Drawable
    {
        /// <summary>
        /// The camera used to render all elements in this scene
        /// </summary>
        public Camera Camera { get; } = new Camera();

        protected override DrawNode CreateDrawNode() => new SceneRootDrawNode();
        
        /// <summary>
        /// Creates a draw node for the entire scene
        /// </summary>
        /// <returns></returns>
        public SceneRootDrawNode GenerateDrawNode()
        {
            SceneRootDrawNode n = (SceneRootDrawNode)CreateDrawNode();
            base.ApplyDrawNode(n);

            n.ViewMatrix = Camera.ViewMatrix;
            n.InverseViewMatrix = Camera.InverseViewMatrix;
            n.ProjectionMatrix = Camera.ProjectionMatrix;
            n.ViewportSize = Camera.ViewportSize;

            foreach(var child in Children)
            {
                child.GenerateDrawNodes(n.Children);
            }

            return n;
        }
    }
}