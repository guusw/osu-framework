// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using OpenTK;
using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics;

namespace osu.Framework.Graphics3D.Particles
{
    /// <summary>
    /// Used to access the particle render data in an efficient way
    /// </summary>
    public unsafe class ParticleBufferAccessor : IDisposable
    {
        /// <summary>
        /// Stride for a single particle in floats
        /// </summary>
        public const int ParticleStride = 4 * 3;

        private int index;
        private GCHandle handle;
        private float* startPtr;
        private float* currentPtr;

        public ParticleBufferAccessor(float[] buffer)
        {
            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            startPtr = (float*)handle.AddrOfPinnedObject();
            SetIndex(0);
        }

        public void SetIndex(int index)
        {
            this.index = index;
            currentPtr = startPtr + ParticleStride * index;
        }

        /// <summary>
        /// Particle index to read/write
        /// </summary>
        public int Index
        {
            get { return index; }
            set { SetIndex(value); }
        }

        /// <summary>
        /// Particle rotation
        /// </summary>
        public float Rotation
        {
            set
            {
                Vector2* rotationCoeffs = (Vector2*)(currentPtr + 6);
                if(value == 0.0f)
                    *rotationCoeffs = new Vector2(1.0f, 0.0f);
                else
                    *rotationCoeffs = new Vector2((float)Math.Cos(value), (float)Math.Sin(value));
            }
        }

        /// <summary>
        /// Particle position
        /// </summary>
        public Vector3 Position
        {
            get { return *(Vector3*)currentPtr; }
            set { *(Vector3*)currentPtr = value; }
        }
        
        /// <summary>
        /// Particle size
        /// </summary>
        public Vector2 Size
        {
            get { return *(Vector2*)(currentPtr+4); }
            set { *(Vector2*)(currentPtr + 4) = value; }
        }
        
        /// <summary>
        /// Particle colour
        /// </summary>
        public Color4 Colour
        {
            get { return *(Color4*)(currentPtr + 8); }
            set { *(Color4*)(currentPtr + 8) = value; }
        }

        public void Dispose()
        {
            handle.Free();
        }
    }
}