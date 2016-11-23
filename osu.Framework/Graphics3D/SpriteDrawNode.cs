// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using OpenTK;

namespace osu.Framework.Graphics3D
{
    public class SpriteDrawNode : DrawNode
    {
        public Texture Texture;
        public Matrix4 WorldMatrix;
        public Shader SpriteShader;
    }
}