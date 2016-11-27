// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using osu.Framework.DebugUtils;
using OpenTK.Graphics.OpenGL;

namespace osu.Framework.Graphics.OpenGL.Textures
{
    public class BufferTextureGL : IDisposable
    {
        private int textureId;
        private int bufferId;
        private int currentBufferSize = 0;
        
        protected virtual void Dispose(bool isDisposing)
        {
        }

        public void Dispose()
        {
            if(textureId != 0)
            {
                GL.DeleteTexture(textureId);
                GL.DeleteBuffer(bufferId);
                textureId = 0;
            }
        }

        /// <summary>
        /// Current size of the buffer in bytes
        /// </summary>
        public int Length => currentBufferSize;

        public void SetData(float[] data)
        {
            // We should never run raw OGL calls on another thread than the main thread due to race conditions.
            ThreadSafety.EnsureDrawThread();

            EnsureCreated();

            int byteLength = data.Length * 4;

            // Set buffer data
            if(currentBufferSize < byteLength)
                Resize(byteLength);

            // Update
            GL.BindBuffer(BufferTarget.TextureBuffer, bufferId);
            GL.BufferSubData(BufferTarget.TextureBuffer, IntPtr.Zero, byteLength, data);
        }

        /// <summary>
        /// Rezizes the buffer to the target size in bytes
        /// </summary>
        /// <param name="targetSize"></param>
        public void Resize(int targetSize)
        {
            if(targetSize != currentBufferSize)
            {
                EnsureCreated();

                // We should never run raw OGL calls on another thread than the main thread due to race conditions.
                ThreadSafety.EnsureDrawThread();

                GL.BindBuffer(BufferTarget.TextureBuffer, bufferId);

                // Reallocate
                GL.BufferData(BufferTarget.TextureBuffer, targetSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                currentBufferSize = targetSize;
            }
        }

        public bool Bind()
        {
            Debug.Assert(textureId > 0);
            GL.BindTexture(TextureTarget.TextureBuffer, textureId);
            return true;
        }

        private void EnsureCreated()
        {
            if(textureId == 0)
            {
                textureId = GL.GenTexture();
                bufferId = GL.GenBuffer();

                // Initial buffer data
                Resize(128);

                // Bind buffer to texture
                GL.BindTexture(TextureTarget.TextureBuffer, textureId);
                GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.Rgba32f, bufferId);
            }
        }
    }
}