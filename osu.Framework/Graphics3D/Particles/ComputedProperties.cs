// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Drawing.Drawing2D;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using OpenTK.Graphics;

namespace osu.Framework.Graphics3D.Particles
{
    /// <summary>
    /// An updater that writes some state directly to the renderdata based on the particles phase
    /// </summary>
    public abstract class ComputedProperty : ParticleParameter
    {
        /// <summary>
        /// Compute particle properties based on their phase
        /// </summary>
        /// <param name="pool">Pool of particles to process</param>
        /// <param name="bufferAccessor">RenderData that receives the properties, the accessor will be reset to 0</param>
        public abstract void ComputeProperties(ParticlePool pool, ParticleBufferAccessor bufferAccessor);
    }

    public class AlphaFade : ComputedProperty
    {
        public float Start = 1.0f;
        public float End = 0.0f; 
        public EasingTypes Easing = EasingTypes.OutCubic;

        public override void ComputeProperties(ParticlePool pool, ParticleBufferAccessor bufferAccessor)
        {
            foreach(var i in pool.AliveParticles)
            {
                var colour = bufferAccessor.Colour;
                float fade = Interpolation.ValueAt(pool.Particles[i].Phase, Start, End, 0.0f, 1.0f, Easing);
                colour.A *= fade;
                bufferAccessor.Colour = colour;
                bufferAccessor.Index++;
            }
        }
    }

    public class SizeFade : ComputedProperty
    {
        public float Start = 1.0f;
        public float End = 0.0f;
        public EasingTypes Easing = EasingTypes.OutCubic;

        public override void ComputeProperties(ParticlePool pool, ParticleBufferAccessor bufferAccessor)
        {
            foreach(var i in pool.AliveParticles)
            {
                var size = bufferAccessor.Size;
                float fade = Interpolation.ValueAt(pool.Particles[i].Phase, Start, End, 0.0f, 1.0f, Easing);
                size *= fade;
                bufferAccessor.Size = size;
                bufferAccessor.Index++;
            }
        }
    }
}