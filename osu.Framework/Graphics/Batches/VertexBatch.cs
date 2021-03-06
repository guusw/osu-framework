﻿// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Statistics;

namespace osu.Framework.Graphics.Batches
{
    public abstract class VertexBatch<T> : IVertexBatch where T : struct, IEquatable<T>
    {
        public List<VertexBuffer<T>> VertexBuffers = new List<VertexBuffer<T>>();

        /// <summary>
        /// The number of vertices in each VertexBuffer.
        /// </summary>
        public int Size { get; }

        private int changeBeginIndex = -1;
        private int changeEndIndex = -1;

        private int currentVertexBuffer;
        private int currentVertex;
        private int lastVertex;

        private int maxBuffers;

        private VertexBuffer<T> CurrentVertexBuffer => VertexBuffers[currentVertexBuffer];

        protected VertexBatch(int bufferSize, int maxBuffers)
        {
            // Vertex buffers of size 0 don't make any sense. Let's not blindly hope for good behavior of OpenGL.
            Debug.Assert(bufferSize > 0);

            Size = bufferSize;
            this.maxBuffers = maxBuffers;
        }

        #region Disposal

        ~VertexBatch()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
                foreach (VertexBuffer<T> vbo in VertexBuffers)
                    vbo.Dispose();
        }

        #endregion

        public void ResetCounters()
        {
            changeBeginIndex = -1;
            currentVertexBuffer = 0;
            currentVertex = 0;
            lastVertex = 0;
        }

        protected abstract VertexBuffer<T> CreateVertexBuffer();

        public void Add(T v)
        {
            GLWrapper.SetActiveBatch(this);

            while (currentVertexBuffer >= VertexBuffers.Count)
                VertexBuffers.Add(CreateVertexBuffer());

            VertexBuffer<T> vertexBuffer = CurrentVertexBuffer;

            if (!vertexBuffer.Vertices[currentVertex].Equals(v))
            {
                if (changeBeginIndex == -1)
                    changeBeginIndex = currentVertex;

                changeEndIndex = currentVertex + 1;
            }

            vertexBuffer.Vertices[currentVertex] = v;
            ++currentVertex;

            if (currentVertex >= vertexBuffer.Vertices.Length)
            {
                Draw();
                FrameStatistics.Increment(StatisticsCounterType.VBufOverflow);
                lastVertex = currentVertex = 0;
            }
        }

        /// <summary>
        /// Draw regular
        /// </summary>
        /// <returns></returns>
        public int Draw()
        {
            return DrawInstanced(0);
        }
        
        /// <param name="instanceCount">if >0 will draw instanced geometry</param>
        /// <returns></returns>
        public int DrawInstanced(int instanceCount)
        {
            if (currentVertex == lastVertex)
                return 0;

            VertexBuffer<T> vertexBuffer = CurrentVertexBuffer;
            if (changeBeginIndex >= 0)
                vertexBuffer.UpdateRange(changeBeginIndex, changeEndIndex);

            if(instanceCount > 0)
                vertexBuffer.DrawRangeInstanced(lastVertex, currentVertex, instanceCount);
            else
                vertexBuffer.DrawRange(lastVertex, currentVertex);

            int count = currentVertex - lastVertex;

            // When using multiple buffers we advance to the next one with every draw to prevent contention on the same buffer with future vertex updates.
            //TODO: let us know if we exceed and roll over to zero here.
            currentVertexBuffer = (currentVertexBuffer + 1) % maxBuffers;
            currentVertex = 0;

            lastVertex = currentVertex;
            changeBeginIndex = -1;

            FrameStatistics.Increment(StatisticsCounterType.DrawCalls);
            FrameStatistics.Increment(StatisticsCounterType.VerticesDraw, count);

            return count;
        }
    }
}
