// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using RectangleF = System.Drawing.RectangleF;

namespace osu.Framework.Graphics3D
{
    public class SpriteDrawNode3D : DrawNode3D
    {
        public Texture Texture;
        public Shader Shader;
        public RectangleF Rectangle;

        public override void Draw()
        {
            base.Draw();
            Shader.Bind();

            Texture.Draw(new Quad(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height), ColourInfo);

            Shader.Unbind();
        }
    }
}