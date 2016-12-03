// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using OpenTK.Graphics;

namespace osu.Framework.Graphics3D
{
    public class Render3DContainerDrawNode : BufferedContainerDrawNode
    {
        public SceneRootDrawNode Scene;
        public Color4 ClearColor;

        protected override void DrawContents(Action<TexturedVertex2D> vertexAction)
        {
            Scene?.Draw();
        }
    }
}