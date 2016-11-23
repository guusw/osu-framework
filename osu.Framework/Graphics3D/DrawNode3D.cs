// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Shaders;
using OpenTK;

namespace osu.Framework.Graphics3D
{
    /// <summary>
    /// 3D draw node
    /// </summary>
    public class DrawNode3D
    {
        public Matrix4 WorldMatrix;
        public BlendingInfo Blending;
        public ColourInfo ColourInfo;

        public virtual void Draw(IVertexBatch vertexBatch)
        {
            Shader.SetGlobalProperty("g_WorldMatrix", WorldMatrix);
            GLWrapper.SetBlend(Blending);
        }
    }
}