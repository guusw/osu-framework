// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace osu.Framework.Graphics3D.Particles
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Particle
    {
        public Vector3 Position;
        public Vector2 Size;
        public Vector3 Velocity;
        public Color4 Colour;
        public float Rotation;
        public float Age;
        public float Duration;
        public float Phase;
        public float Depth; // Sorting
        public bool IsAlive => Duration > 0.0f;
    }

    public class ParticlePool
    {
        public Particle[] Particles;
        public List<int> AliveParticles = new List<int>();

        private readonly Queue<int> freeList = new Queue<int>();

        public ParticlePool(int initial = 100)
        {
            Resize(initial);
        }

        public int Available => freeList.Count;
        
        public int Allocate()
        {
            int particle = freeList.Dequeue();
            AliveParticles.Add(particle);
            return particle;
        }

        public void Free(int particle)
        {
            Particles[particle].Duration = 0.0f;
            freeList.Enqueue(particle);
            AliveParticles.Remove(particle);
        }

        public void Resize(int target)
        {
            int existing = Particles?.Length ?? 0;
            Array.Resize(ref Particles, target);

            if(target < existing) // Shrink
            {
                // Rebuild free list and alive list
                AliveParticles.Clear();
                freeList.Clear();
                for(int i = 0; i < target; i++)
                {
                    if(!Particles[i].IsAlive)
                        freeList.Enqueue(i);
                    else
                        AliveParticles.Add(i);
                }
            }
            else // Enlarge
            {
                // Add new particles to free list
                for(int i = existing; i < target; i++)
                    Free(i);
            }
        }
    }

    public class ParticleSystem : Drawable3D
    {
        /// <summary>
        /// How many particles to spawn per second
        /// </summary>
        public float EmissionRate = 20.0f;
        public Texture Texture;

        private BufferTextureGL gpuBuffer;
        private Shader shader;
        private ParticlePool pool = new ParticlePool();
        private float emissionTimer = 0.0f;

        private PositionInitializer positionInitializer;
        private DurationInitializer durationInitializer;
        private Initializer velocityInitializer;
        private Initializer colourInitializer;
        private Initializer rotationInitializer;
        private SizeInitializer sizeInitializer;
        private List<Initializer> initializers = new List<Initializer>();
        private List<Updater> updaters = new List<Updater>();
        private List<ComputedProperty> computedProperties = new List<ComputedProperty>();
        private GlobalUpdater globalUpdater = new GlobalUpdater();
        
        public ParticleSystem()
        {
            globalUpdater.OnAttach(this);

            // Default initializers
            AttachInitializer(new PositionInitializer(), ref positionInitializer);
            AttachInitializer(new DurationInitializer(), ref durationInitializer);
            AttachInitializer(new SizeInitializer(), ref sizeInitializer);
            AttachInitializer(new ColourInitializer(), ref colourInitializer);
        }

        public override void Dispose()
        {
            base.Dispose();
            gpuBuffer?.Dispose();
        }

        public PositionInitializer PositionInitializer
        {
            get { return positionInitializer; }
            set { AttachInitializer(value, ref positionInitializer); }
        }

        public DurationInitializer DurationInitializer
        {
            get { return durationInitializer; }
            set { AttachInitializer(value, ref durationInitializer); }
        }

        public Initializer VelocityInitializer
        {
            get { return velocityInitializer; }
            set { AttachInitializer(value, ref velocityInitializer); }
        }

        public Initializer ColourInitializer
        {
            get { return colourInitializer; }
            set { AttachInitializer(value, ref colourInitializer); }
        }

        public Initializer RotationInitializer
        {
            get { return rotationInitializer; }
            set { AttachInitializer(value, ref rotationInitializer); }
        }

        public SizeInitializer SizeInitializer
        {
            get { return sizeInitializer; }
            set { AttachInitializer(value, ref sizeInitializer); }
        }

        public int MaximumParticles
        {
            get { return pool.Particles.Length; }
            set { pool.Resize(value); }
        }

        /// <summary>
        /// Transforms such as color, velocity that get applied to the particle state
        /// </summary>
        public IEnumerable<Updater> ParticleUpdaters
        {
            get { return updaters; }
            set { AddUpdaters(value);}
        }
        
        /// <summary>
        /// Computed properties over the lifetime of a particle
        /// </summary>
        public IEnumerable<ComputedProperty> ComputedProperties
        {
            get { return computedProperties; }
            set { AddComputedProperties(value); }
        }

        public override unsafe void Update()
        {
            base.Update();
        }

        public void AddUpdaters(IEnumerable<Updater> updaters)
        {
            foreach(var updater in updaters)
                AddUpdater(updater);
        }

        public void AddUpdater(Updater updater)
        {
            if(updater.System != null) throw new InvalidOperationException();
            updater.OnAttach(this);
            updaters.Add(updater);
        }

        public void RemoveUpdater(Updater updater)
        {
            if(!updaters.Contains(updater)) throw new InvalidOperationException();
            updaters.Remove(updater);
            updater?.OnDetach();
        }

        public void ClearUpdaters()
        {
            foreach(var updater in updaters)
                updater.OnDetach();
            updaters.Clear();
        }

        public void AddComputedProperties(IEnumerable<ComputedProperty> properties)
        {
            foreach(var property in properties)
                AddComputedProperty(property);
        }

        public void AddComputedProperty(ComputedProperty property)
        {
            if(property.System != null) throw new InvalidOperationException();
            property.OnAttach(this);
            computedProperties.Add(property);
        }

        public void RemoveComputedProperty(ComputedProperty property)
        {
            if(!computedProperties.Contains(property)) throw new InvalidOperationException();
            computedProperties.Remove(property);
            property?.OnDetach();
        }

        public void ClearComputedProperties()
        {
            foreach(var p in this.computedProperties)
                p.OnDetach();
            computedProperties.Clear();
        }

        protected void UpdateParticles()
        {
            float deltaTime = (float)(Time.Elapsed / 1000.0);

            // Set depth enbled state
            globalUpdater.ProcessDepth = BlendingMode == BlendingMode.Mixture;
            if(globalUpdater.ProcessDepth)
            {
                globalUpdater.CameraForward = Scene.Camera.InverseViewMatrix.Row2.Xyz;
            }

            // Simulate all particles
            globalUpdater.Update(pool, deltaTime, 0, pool.Particles.Length);
            foreach(var updater in updaters)
                updater.Update(pool, deltaTime, 0, pool.Particles.Length);
            
            // Spawn new particles
            emissionTimer += EmissionRate * deltaTime;
            if(emissionTimer > 1.0f)
            {
                int particlesToSpawn = (int)Math.Floor(emissionTimer);
                emissionTimer -= particlesToSpawn;
                float simulationStep = 1.0f / particlesToSpawn;
                for(int i = 0; i < particlesToSpawn; i++)
                {
                    if(pool.Available == 0)
                        break;

                    // Spawn particle
                    int index = pool.Allocate();

                    foreach(var initializer in initializers)
                        initializer.Initialize(ref pool.Particles[index]);

                    // Simulate for a certain time to not spawn multiple particles at the same place, etc.
                    foreach(var updater in updaters)
                        updater.Update(pool, simulationStep * i, index, 1);
                    globalUpdater.Update(pool, simulationStep * i, index, 1);
                }
            }

            // Sort particles
            if(globalUpdater.ProcessDepth)
                pool.AliveParticles.Sort((l, r) => pool.Particles[l].Depth.CompareTo(pool.Particles[l].Depth));
        }

        protected override DrawNode3D CreateDrawNode() => new ParticleSystemDrawNode();

        protected override void ApplyDrawNode(DrawNode3D node)
        {
            base.ApplyDrawNode(node);
            var n = (ParticleSystemDrawNode)node;
            n.Shader = shader;
            n.Texture = Texture;
            n.Pool = pool;
            n.InverseViewMatrix = Scene.Camera.InverseViewMatrix;

            if(gpuBuffer == null)
                gpuBuffer = new BufferTextureGL();
            n.Buffer = gpuBuffer;


            // Create data as matrices
            // TODO: Reuse array
            n.BufferData = new float[ParticleBufferAccessor.ParticleStride * pool.AliveParticles.Count];
            using(ParticleBufferAccessor bufferAccessor = new ParticleBufferAccessor(n.BufferData))
            {
                // Write base properties
                // TODO: Detect which properties have computed variant and skip those here
                bufferAccessor.Index = 0;
                foreach(var i in pool.AliveParticles)
                {
                    bufferAccessor.Position = pool.Particles[i].Position;
                    bufferAccessor.Rotation = pool.Particles[i].Rotation;
                    bufferAccessor.Colour = pool.Particles[i].Colour;
                    bufferAccessor.Size = pool.Particles[i].Size;
                    bufferAccessor.Index++;
                }
                
                // Write custom properties
                foreach(var property in ComputedProperties)
                {
                    bufferAccessor.Index = 0;
                    property.ComputeProperties(pool, bufferAccessor);
                }
            }

            n.InstanceCount = pool.AliveParticles.Count;

            UpdateParticles();
        }
        
        private void AttachInitializer<TType>(TType newParameter, ref TType target) where TType : Initializer
        {
            if(target != null)
            {
                target.OnDetach();
                initializers.Remove(target);
            }
            target = newParameter;
            if(target != null)
            {
                target.OnAttach(this);
                initializers.Add(target);
            }
        }

        [BackgroundDependencyLoader]
        void Load(ShaderManager shaderManager)
        {
            shader = shaderManager.Load(new ShaderDescriptor(VertexShaderDescriptor.InstancedParticle,
                FragmentShaderDescriptor.Texture));
        }
    }
}