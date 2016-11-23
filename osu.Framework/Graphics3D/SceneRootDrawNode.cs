// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
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
    }
}