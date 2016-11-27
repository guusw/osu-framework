// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.Graphics3D.Particles
{
    /// <summary>
    /// Initializes particle properties
    /// </summary>
    public abstract class Initializer : ParticleParameter
    {
        public abstract void Initialize(ref Particle particle);
    }

    public class PositionInitializer : Initializer
    {
        public override void Initialize(ref Particle particle)
        {
            particle.Position = System.WorldMatrix.ExtractTranslation();
        }
    }

    public class DurationInitializer : Initializer
    {
        private float minimum = 1.0f;
        private float maximum = 1.0f;
        private float delta = 0.0f;
        private Random random = new Random();

        public float Minimum
        {
            get { return minimum; }
            set { minimum = value; UpdateDelta(); }
        }

        public float Maximum
        {
            get { return maximum; }
            set { maximum = value; UpdateDelta(); }
        }

        public override void Initialize(ref Particle particle)
        {
            particle.Age = 0.0f; // Reset age
            particle.Phase = 0.0f;
            particle.Duration = (float)random.NextDouble() * delta + minimum;
        }

        private void UpdateDelta()
        {
            delta = maximum - minimum;
        }
    }

    public class SizeInitializer : Initializer
    {
        private Vector2 minimum = new Vector2(1.0f);
        private Vector2 maximum = new Vector2(1.0f);
        private Vector2 delta = Vector2.Zero;
        private Random random = new Random();

        public Vector2 Minimum
        {
            get { return minimum; }
            set { minimum = value; UpdateDelta(); }
        }

        public Vector2 Maximum
        {
            get { return maximum; }
            set { maximum = value; UpdateDelta(); }
        }

        public float MinimumUniform
        {
            get { return minimum.X; }
            set { minimum = new Vector2(value); UpdateDelta(); }
        }

        public float MaximumUniform
        {
            get { return maximum.X; }
            set { maximum = new Vector2(value); UpdateDelta();}
        }


        public override void Initialize(ref Particle particle)
        {
            particle.Size.X = (float)random.NextDouble() * delta.X + minimum.X;
            particle.Size.Y = (float)random.NextDouble() * delta.Y + minimum.Y;
        }

        private void UpdateDelta()
        {
            delta = maximum - minimum;
        }
    }

    public class ColourInitializer : Initializer
    {
        public Color4 Colour = Color4.White;

        public ColourInitializer() {}

        public ColourInitializer(Color4 colour)
        {
            this.Colour = colour;
        }

        public override void Initialize(ref Particle particle)
        {
            particle.Colour = Colour;
        }
    }
    
    public class RotationInitializer : Initializer
    {
        private float minimum = 0.0f;
        private float maximum = 0.0f;
        private float delta = 0.0f;
        private Random random = new Random();

        public float Minimum
        {
            get { return minimum; }
            set { minimum = value; UpdateDelta(); }
        }

        public float Maximum
        {
            get { return maximum; }
            set { maximum = value; UpdateDelta(); }
        }

        public override void Initialize(ref Particle particle)
        {
            particle.Rotation = (float)random.NextDouble() * delta + minimum;
        }

        private void UpdateDelta()
        {
            delta = maximum - minimum;
        }
    }

    public abstract class VelocityInitializer : Initializer
    {
        protected Random Random = new Random();
        private float minimumVelocity = 1.0f;
        private float maximumVelocity = 1.0f;
        private float deltaVelocity = 0.0f;

        public float MinimumVelocity
        {
            get { return minimumVelocity; }
            set { minimumVelocity = value; UpdateDelta(); }
        }

        public float MaximumVelocity
        {
            get { return maximumVelocity; }
            set { maximumVelocity = value; UpdateDelta(); }
        }
        
        private void UpdateDelta()
        {
            deltaVelocity = maximumVelocity - minimumVelocity;
        }

        public float GenerateVelocity()
        {
            return (float)Random.NextDouble() * deltaVelocity + minimumVelocity;
        }
    }

    public class ConeVelocityInitializer : VelocityInitializer
    {
        /// <summary>
        /// Angle of the cone, based around the direction of the particle system
        /// </summary>
        public float Angle;


        public override void Initialize(ref Particle particle)
        {
            float r = (float)Random.NextDouble() * Angle;
            float t = (float)Random.NextDouble() * MathHelper.TwoPi;
            float z = (float)Math.Cos(r);
            float invZSqr = (float)Math.Sqrt(1.0f - (z*z));
            Vector3 dir = new Vector3(invZSqr * (float)Math.Cos(t), invZSqr * (float)Math.Sin(t), z);

            var systemMatrix = System.WorldMatrix;

            particle.Velocity = systemMatrix.Row0.Xyz * dir.X + systemMatrix.Row1.Xyz * dir.Y + systemMatrix.Row2.Xyz * dir.Z;
            particle.Velocity *= GenerateVelocity();
        }
    }
}