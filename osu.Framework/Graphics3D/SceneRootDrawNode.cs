// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Shaders;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace osu.Framework.Graphics3D
{
    public class SceneRootDrawNode : DrawNode3D
    {
        public Matrix4 ViewMatrix;
        public Matrix4 InverseViewMatrix;
        public Matrix4 ProjectionMatrix;
        public Vector2 ViewportSize;
        public List<DrawNode3D> Children = new List<DrawNode3D>();

        private Vector3 cameraForward;

        public override void Draw(IVertexBatch vertexBatch)
        {
            base.Draw(vertexBatch);

            // Set camera uniforms
            Shader.SetGlobalProperty(@"g_ProjMatrix", ProjectionMatrix);
            Shader.SetGlobalProperty(@"g_ViewMatrix", ViewMatrix);
            Shader.SetGlobalProperty(@"g_InvViewMatrix", InverseViewMatrix);

            // Invert culling
            //GL.CullFace(CullFaceMode.Front); // Flip culling for 3D

            foreach(var child in Children)
            {
                child.Draw(vertexBatch);
            }

            // Flush batch before disabling state
            GLWrapper.FlushCurrentBatch();

            // Restore 3D states
            //GL.CullFace(CullFaceMode.Back);
        }
        
        /// <summary>
        /// Perform depth sorting
        /// </summary>
        public void Sort()
        {
            if(Children.Count > 0)
            {
                // Cache forward vector of camera
                cameraForward = InverseViewMatrix.Row2.Xyz;

                // Perform broad phase sorting by transparency key
                Children.Sort((l, r) => l.TransparencyGroup.CompareTo(r.TransparencyGroup));
                
                int start = 0;
                int currentGroup = Children[0].TransparencyGroup;
                for(int i = 0; i < Children.Count;)
                {
                    var nextIndex = i + 1;
                    if(nextIndex >= Children.Count)
                    {
                        SortTransparencyGroup(start, nextIndex - start);
                        break; // Sort last
                    }

                    var next = Children[i + 1];
                    if(next.TransparencyGroup > currentGroup)
                    {
                        SortTransparencyGroup(start, nextIndex - start);
                        currentGroup = next.TransparencyGroup;
                        start = nextIndex;
                    }
                    i = nextIndex;
                }
            }
        }
        
        private void SortTransparencyGroup(int start, int count)
        {
            if(count > 1)
            {
                // Calculate depth for every item
                for(int i = start; i < (start + count); i++)
                {
                    var node = Children[i];
                    node.Depth = Vector3.Dot(cameraForward, node.WorldMatrix.ExtractTranslation());
                }

                // Sort by depth
                Children.Sort(start, count, DrawNodeDepthComparer.Instance);
            }
        }

        private class DrawNodeDepthComparer : IComparer<DrawNode3D>
        {
            public static DrawNodeDepthComparer Instance { get;} = new DrawNodeDepthComparer();

            public int Compare(DrawNode3D x, DrawNode3D y)
            {
                return x.Depth.CompareTo(y.Depth);
            }
        }
    }
}