// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using OpenTK;

namespace osu.Framework.Graphics3D.Particles
{
    /// <summary>
    /// An animated property on a particle
    /// </summary>
    public abstract class Updater : ParticleParameter
    {
        /// <summary>
        /// Update particles
        /// </summary>
        /// <param name="pool">Pool of particles to process</param>
        /// <param name="delta">Delta time in seconds</param>
        /// <param name="start">Starting particle index in the pool</param>
        /// <param name="count">Number of particles to process</param>
        public abstract void Update(ParticlePool pool, float delta, int start, int count);
    }
    
    internal class GlobalUpdater : Updater
    {
        public Vector3 CameraForward;
        public bool ProcessDepth = false;

        public override void Update(ParticlePool pool, float delta, int start, int count)
        {
            for(int i = start; i < count; i++)
            {
                if(pool.Particles[i].IsAlive)
                {
                    // Update age
                    pool.Particles[i].Age += delta;
                    pool.Particles[i].Phase = pool.Particles[i].Age / pool.Particles[i].Duration;
                    if(pool.Particles[i].Age > pool.Particles[i].Duration)
                    {
                        pool.Free(i); // Release particle
                        continue;
                    }

                    // Update velocity
                    pool.Particles[i].Position += pool.Particles[i].Velocity * delta;

                    if(ProcessDepth)
                    {
                        pool.Particles[i].Depth = Vector3.Dot(CameraForward, pool.Particles[i].Position);
                    }
                }
            }
        }
    }

    public class Gravity : Updater
    {
        public Vector3 Force = new Vector3(0.0f, -9.81f, 0.0f);

        public override void Update(ParticlePool pool, float delta, int start, int count)
        {
            Vector3 scaledForce = Force * delta;
            for(int i = start; i < count; i++)
            {
                pool.Particles[i].Velocity += scaledForce;
            }
        }
    }

    public class ConstantRotationSpeed : Updater
    {
        public float Speed = MathHelper.TwoPi;

        public override void Update(ParticlePool pool, float delta, int start, int count)
        {
            float scaledSpeed = Speed * delta;
            for(int i = start; i < count; i++)
            {
                if(pool.Particles[i].IsAlive)
                {
                    pool.Particles[i].Rotation += scaledSpeed;
                }
            }
        }
    }

    public class VelocityDecay : Updater
    {
        public float Decay = 0.1f;

        public override void Update(ParticlePool pool, float delta, int start, int count)
        {
            float multiplier = (1.0f - Decay * delta);
            for(int i = start; i < count; i++)
            {
                if(pool.Particles[i].IsAlive)
                {
                    pool.Particles[i].Velocity *= multiplier;
                }
            }
        }
    }
}
