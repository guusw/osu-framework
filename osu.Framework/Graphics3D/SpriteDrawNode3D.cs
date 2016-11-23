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

        public override void Draw(IVertexBatch vertexBatch)
        {
            base.Draw(vertexBatch);
            Shader.Bind();

            Texture.Draw(new Quad(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height), ColourInfo);

            //GLWrapper.BindTexture(Texture.TextureGL);
            //// TODO: Calculate sub-texture coordinates for not single textures
            //
            //if(spriteBatch == null)
            //{
            //    spriteBatch = new QuadBatch<TexturedVertex3D>(512, 128);
            //}
            //
            //spriteBatch.Add(new TexturedVertex3D
            //{
            //    Position = new Vector3(Rectangle.Left, Rectangle.Top, 0.0f),
            //    Colour = Colour.TopLeft.Linear,
            //    TexturePosition = new Vector2(0.0f, 1.0f)
            //});
            //spriteBatch.Add(new TexturedVertex3D
            //{
            //    Position = new Vector3(Rectangle.Right, Rectangle.Top, 0.0f),
            //    Colour = Colour.TopRight.Linear,
            //    TexturePosition = new Vector2(1.0f, 1.0f)
            //});
            //spriteBatch.Add(new TexturedVertex3D
            //{
            //    Position = new Vector3(Rectangle.Right, Rectangle.Bottom, 0.0f),
            //    Colour = Colour.BottomRight.Linear,
            //    TexturePosition = new Vector2(1.0f, 0.0f)
            //});
            //spriteBatch.Add(new TexturedVertex3D
            //{
            //    Position = new Vector3(Rectangle.Left, Rectangle.Bottom, 0.0f),
            //    Colour = Colour.BottomLeft.Linear,
            //    TexturePosition = new Vector2(0.0f, 0.0f)
            //});

            Shader.Unbind();
        }
    }
}