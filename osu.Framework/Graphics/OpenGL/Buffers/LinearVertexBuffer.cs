﻿// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using OpenTK.Graphics.OpenGL;

namespace osu.Framework.Graphics.OpenGL.Buffers
{
    static class LinearIndexData
    {
        static LinearIndexData()
        {
            GL.GenBuffers(1, out EboId);
        }

        public static readonly int EboId;
        public static int MaxAmountIndices;
    }

    /// <summary>
    /// This type of vertex buffer lets the ith vertex be referenced by the ith index.
    /// </summary>
    public class LinearVertexBuffer<T> : VertexBuffer<T> where T : struct, IEquatable<T>
    {
        private readonly PrimitiveType type;

        public LinearVertexBuffer(int amountVertices, PrimitiveType type, BufferUsageHint usage)
            : base(amountVertices, usage)
        {
            this.type = type;

            if (amountVertices > LinearIndexData.MaxAmountIndices)
            {
                ushort[] indices = new ushort[amountVertices];

                for (ushort i = 0; i < amountVertices; i++)
                    indices[i] = i;

                GLWrapper.BindBuffer(BufferTarget.ElementArrayBuffer, LinearIndexData.EboId);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(amountVertices * sizeof(ushort)), indices, BufferUsageHint.StaticDraw);

                LinearIndexData.MaxAmountIndices = amountVertices;
            }
        }

        public override void Bind(bool forRendering)
        {
            base.Bind(forRendering);

            if (forRendering)
                GLWrapper.BindBuffer(BufferTarget.ElementArrayBuffer, LinearIndexData.EboId);
        }

        protected override PrimitiveType Type => type;
    }
}
