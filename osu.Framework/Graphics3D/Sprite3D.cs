// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Caching;

namespace osu.Framework.Graphics3D
{
    /// <summary>
    /// A sprite in 3D space
    /// </summary>
    public class Sprite3D : Drawable3D
    {
        private Shader textureShader;

        /// <summary>
        /// The position and size of this sprite
        /// </summary>
        public RectangleF Rectangle { get; set; } = new RectangleF(-0.5f, -0.5f, 1.0f, 1.0f);

        public Texture Texture { get; set; }

        protected override DrawNode3D CreateDrawNode() => new SpriteDrawNode3D();

        protected override void ApplyDrawNode(DrawNode3D node)
        {
            base.ApplyDrawNode(node);
            var n = (SpriteDrawNode3D)node;
            n.Texture = Texture ?? Texture.WhitePixel;
            n.Shader = textureShader;
            n.Rectangle = Rectangle;
        }
        
        [BackgroundDependencyLoader]
        private void Load(ShaderManager shaders)
        {
            if(textureShader == null)
                textureShader = shaders?.Load(new ShaderDescriptor(VertexShaderDescriptor.WorldViewProjection, FragmentShaderDescriptor.Texture));
        }
    }
}