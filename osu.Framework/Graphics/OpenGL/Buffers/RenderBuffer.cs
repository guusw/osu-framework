// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace osu.Framework.Graphics.OpenGL.Buffers
{

    public class RenderBuffer : IDisposable
    {
        private static Dictionary<RenderbufferStorage, Stack<RenderBufferInfo>> renderBufferCache = new Dictionary<RenderbufferStorage, Stack<RenderBufferInfo>>();

        public Vector2 Size = Vector2.One;
        public RenderbufferStorage Format { get; }

        private RenderBufferInfo info;
        private bool isDisposed;

        public RenderBuffer(RenderbufferStorage format)
        {
            Format = format;
        }

        #region Disposal

        ~RenderBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            isDisposed = true;

            Unbind();
        }

        #endregion

        /// <summary>
        /// Binds the renderbuffer to the specfied framebuffer.
        /// </summary>
        /// <param name="frameBuffer">The framebuffer this renderbuffer should be bound to.</param>
        internal void Bind(int frameBuffer)
        {
            // Check if we're already bound
            if (info != null)
                return;

            if (!renderBufferCache.ContainsKey(Format))
                renderBufferCache[Format] = new Stack<RenderBufferInfo>();

            // Make sure we have renderbuffers available
            if (renderBufferCache[Format].Count == 0)
                renderBufferCache[Format].Push(new RenderBufferInfo
                {
                    RenderBufferID = GL.GenRenderbuffer(),
                    FrameBufferID = -1
                });

            // Get a renderbuffer from the cache
            info = renderBufferCache[Format].Pop();

            // Check if we need to update the size
            if (info.Size != Size)
            {
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, info.RenderBufferID);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Format, (int)Math.Ceiling(Size.X), (int)Math.Ceiling(Size.Y));

                info.Size = Size;
            }

            // For performance reasons, we only need to re-bind the renderbuffer to
            // the framebuffer if it is not already attached to it
            if (info.FrameBufferID != frameBuffer)
            {
                // Make sure the framebuffer we want to attach to is bound
                int lastFrameBuffer = GLWrapper.BindFrameBuffer(frameBuffer);

                switch (Format)
                {
                    case RenderbufferStorage.DepthComponent16:
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, info.RenderBufferID);
                        break;
                    case RenderbufferStorage.Rgba8:
                    case RenderbufferStorage.Rgba4:
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, info.RenderBufferID);
                        break;
                    case RenderbufferStorage.StencilIndex8:
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, info.RenderBufferID);
                        break;
                    default:
                        throw new InvalidOperationException("RenderBuffer format not supported");
                }

                GLWrapper.BindFrameBuffer(lastFrameBuffer);
            }

            info.FrameBufferID = frameBuffer;
        }

        /// <summary>
        /// Unbinds the renderbuffer.
        /// <para>The renderbuffer will remain internally attached to the framebuffer.</para>
        /// </summary>
        internal void Unbind()
        {
            if (info == null)
                return;

            // Return the renderbuffer to the cache
            renderBufferCache[Format].Push(info);

            info = null;
        }

        private class RenderBufferInfo
        {
            public int RenderBufferID;
            public int FrameBufferID;
            public Vector2 Size;
        }
    }
}
