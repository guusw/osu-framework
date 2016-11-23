// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
using osu.Framework.Graphics.Shaders;
using OpenTK;

namespace osu.Framework.Graphics3D
{
    public class SceneRootDrawNode : DrawNode
    {
        public Matrix4 ViewMatrix;
        public Matrix4 InverseViewMatrix;
        public Matrix4 ProjectionMatrix;
        public Vector2 ViewportSize;
        public List<DrawNode> Children = new List<DrawNode>();

        public override void Draw()
        {
            base.Draw();

            Shader.SetGlobalProperty(@"g_ProjMatrix", ProjectionMatrix);
            Shader.SetGlobalProperty(@"g_ViewMatrix", ViewMatrix);
            Shader.SetGlobalProperty(@"g_InvViewMatrix", InverseViewMatrix);

            foreach(var child in Children)
            {
                child.Draw();
            }
        }
    }
}