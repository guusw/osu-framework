// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;

namespace osu.Framework.Graphics3D
{
    /// <summary>
    /// A sprite in 3D space
    /// </summary>
    public class Sprite : Drawable
    {
        private Shader textureShader;

        public Texture Texture { get; set; }
        
        protected override DrawNode CreateDrawNode() => new SpriteDrawNode();

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);
        }
        
        [BackgroundDependencyLoader]
        private void Load(ShaderManager shaders)
        {
            if(textureShader == null)
                textureShader = shaders?.Load("sh_WVP.vs", "sh_Textured.fs");
        }
    }
}