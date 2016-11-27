using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.Graphics3D.Particles
{
    public class ParticleSystemDrawNode : DrawNode3D
    {
        public Shader Shader;
        public Texture Texture;
        public Matrix4 InverseViewMatrix;
        public ParticlePool Pool;
        public BufferTextureGL Buffer;
        public float[] BufferData;
        public int InstanceCount;

        private static QuadBatch<TexturedVertex2D> particleInstancedBatch;

        public override void Draw(IVertexBatch vertexBatch)
        {
            if(Texture == null)
                return;

            // For billboarding
            Shader.SetGlobalProperty("g_InverseViewMatrix", InverseViewMatrix);

            // Taken from DrawNode3D, since we don't need the world transform for particle systems
            GLWrapper.SetBlend(Blending);

            Shader.Bind();
            
            // Force create a separate batch so we can use instanced rendering
            if(particleInstancedBatch == null)
                particleInstancedBatch = new QuadBatch<TexturedVertex2D>(TexturedVertex2D.Stride * 4, 1);
            
            var colourInfo = new ColourInfo {Colour = Color4.White};
            Texture.Draw(new Quad(-0.5f, -0.5f, 1.0f, 1.0f), colourInfo, spriteBatch: particleInstancedBatch);

            // Set GPU buffer data
            Buffer.SetData(BufferData);

            Buffer.Bind();

            // Render instanced
            particleInstancedBatch.DrawInstanced(InstanceCount);

            Shader.Unbind();
        }
    }
}