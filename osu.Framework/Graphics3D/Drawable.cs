// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

// Based on osu.Framework, adapted for 3D rendering

// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Caching;
using osu.Framework.DebugUtils;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Lists;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Framework.Timing;
using OpenTK;

namespace osu.Framework.Graphics3D
{
    /// <summary>
    /// Drawable, but in 3D
    /// </summary>
    public abstract partial class Drawable : IDisposable, IHasLifetime, ITransformable3D
    {
        private static StopwatchClock Perf = new StopwatchClock(true);
        private const float VisiblityCutoff = 0.0001f;
        
        public volatile LoadState LoadState;

        private List<Drawable> children = new List<Drawable>();

        // Transformation components
        private Vector3 position = Vector3.Zero;
        private Quaternion rotation = Quaternion.Identity;
        private Vector3 rotationPivot = Vector3.Zero;
        private Vector3 scale = Vector3.One;
        private Matrix4 toWorldMatrix = Matrix4.Identity;
        private Cached<Matrix4> worldMatrix = new Cached<Matrix4>();
        private bool isToWorldValid = true;
        private SceneRoot root;
        
        private IFrameBasedClock customClock;
        private IFrameBasedClock clock;

        /// <summary>
        /// A lazily-initialized scheduler used to schedule tasks to be invoked in future Update calls.
        /// </summary>
        private Scheduler scheduler;
        private Thread mainThread;

        private LifetimeList<ITransform> transforms;
        
        public void Dispose()
        {
            foreach(var child in children)
            {
                child.Dispose();
            }
        }

        /// <summary>
        /// The local matrix of this drawable
        /// </summary>
        public Matrix4 ToWorldMatrix
        {
            get
            {
                if(!isToWorldValid)
                    RefreshToWorldMatrix();
                return toWorldMatrix;
            }
            set
            {
                toWorldMatrix = value;
                isToWorldValid = true;

                // Set and decompose into position/rotation
                scale = toWorldMatrix.ExtractScale();
                position = toWorldMatrix.ExtractTranslation();
                rotation = toWorldMatrix.ExtractRotation();
                rotationPivot = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        /// <summary>
        /// The world matrix of this drawable
        /// </summary>
        public Matrix4 WorldMatrix => worldMatrix.EnsureValid()
            ? worldMatrix.Value
            : worldMatrix.Refresh(() =>
            {
                if(Parent == null)
                    return ToWorldMatrix;
                return ToWorldMatrix * Parent.WorldMatrix;
            });

        public Vector3 Scale
        {
            get { return scale; }
            set
            {
                scale = Scale;
                InvalidateToWorldMatrix();
            }
        }

        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                InvalidateToWorldMatrix();
            }
        }

        public Quaternion Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                InvalidateToWorldMatrix();
            }
        }

        public Vector3 RotationPivot
        {
            get { return rotationPivot; }
            set
            {
                rotationPivot = value;
                InvalidateToWorldMatrix();
            }
        }

        public SceneRoot Root
        {
            get { return root; }
            protected set
            {
                root = value; 
                // Propagate root setting to children   
                foreach(var child in Children)
                    child.Root = root;
            }
        }

        public Drawable Parent { get; protected set; }

        /// <summary>
        /// The children of this drawable
        /// </summary>
        public IEnumerable<Drawable> Children
        {
            get { return children; }
            set { Add(value); }
        }

        /// <summary>
        /// Transparency
        /// </summary>
        public float Alpha { get; set; }

        /// <summary>
        /// Colour of this object
        /// </summary>
        public SRGBColour Colour { get; set; }

        public bool IsVisible
        {
            get { return Alpha > VisiblityCutoff; }
        }

        /// <summary>
        /// The time at which this drawable becomes valid (and is considered for drawing).
        /// </summary>
        public double LifetimeStart { get; set; } = double.MinValue;

        /// <summary>
        /// The time at which this drawable is no longer valid (and is considered for disposal).
        /// </summary>
        public double LifetimeEnd { get; set; } = double.MaxValue;

        /// <summary>
        /// Override to add delayed load abilities (ie. using IsAlive)
        /// </summary>
        public virtual bool IsLoaded => LoadState >= LoadState.Loaded;
        
        /// <summary>
        /// Whether this drawable is alive.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                //we have been loaded but our parent has since been nullified
                if(Parent == null && IsLoaded) return false;

                if(LifetimeStart == double.MinValue && LifetimeEnd == double.MaxValue)
                    return true;

                return Time.Current >= LifetimeStart && Time.Current < LifetimeEnd;
            }
        }

        public FrameTimeInfo Time => Clock.TimeInfo;

        public IFrameBasedClock Clock
        {
            get { return clock; }
            set
            {
                customClock = value;
                UpdateClock(customClock);
            }
        }

        /// <summary>
        /// Whether to remove the drawable from its parent's children when it's not alive.
        /// </summary>
        public virtual bool RemoveWhenNotAlive => Parent == null || Time.Current > LifetimeStart;

        protected Scheduler Scheduler
        {
            get
            {
                if(scheduler == null)
                    //mainThread could be null at this point.
                    scheduler = new Scheduler(mainThread);

                return scheduler;
            }
        }


        /// <summary>
        /// The list of transforms applied to this drawable. Initialised on first access.
        /// </summary>
        public LifetimeList<ITransform> Transforms
        {
            get
            {
                if(transforms == null)
                {
                    transforms = new LifetimeList<ITransform>(new TransformTimeComparer());
                    transforms.Removed += transforms_OnRemoved;
                }

                return transforms;
            }
        }

        public bool UpdateSubTree()
        {
            if(LoadState < LoadState.Alive)
                if(!loadComplete())
                    return false;

            transformationDelay = 0;

            UpdateTransforms();

            if(!IsVisible)
                return true;

            // Construct valid local matrix
            if(!isToWorldValid)
            {
                RefreshToWorldMatrix();
            }

            Update();

            return true;
        }

        /// <summary>
        /// Generates a list of drawnodes from this drawable and possible children
        /// </summary>
        /// <param name="target"></param>
        public void GenerateDrawNodes(IList<DrawNode> target)
        {
            // Generate a draw node for this object
            var drawNodeSelf = CreateDrawNode();
            if(drawNodeSelf != null)
            {
                ApplyDrawNode(drawNodeSelf);
            }

            // Generate child draw nodes
            foreach(var child in Children)
            {
                child.GenerateDrawNodes(target);
            }
        }

        public void Add(IEnumerable<Drawable> collection)
        {
            foreach(var drawable in collection)
                Add(drawable);
        }

        public void Add(Drawable drawable)
        {
            children.Add(drawable);
            drawable.Parent = this;
            drawable.Root = Root;
        }

        public void Remove(Drawable drawable, bool dispose = false)
        {
            if(dispose)
                drawable.Dispose();
            drawable.Parent = null;
            drawable.Root = null;
        }

        /// <summary>
        /// Update method
        /// </summary>
        public virtual void Update()
        {
        }

        public void UpdateTime(FrameTimeInfo time)
        {
            throw new NotImplementedException();
        }

        public Task Preload(BaseGame game, Action<Drawable> onLoaded = null)
        {
            if(LoadState == LoadState.NotLoaded)
                return Task.Run(() => PerformLoad(game)).ContinueWith(obj => game.Schedule(() => onLoaded?.Invoke(this)));

            Debug.Assert(LoadState >= LoadState.Loaded, "Preload got called twice on the same Drawable.");
            onLoaded?.Invoke(this);
            return null;
        }

        internal virtual void UpdateClock(IFrameBasedClock clock)
        {
            this.clock = customClock ?? clock;
        }

        /// <summary>
        /// Creates a draw node for this drawable
        /// </summary>
        /// <returns></returns>
        protected abstract DrawNode CreateDrawNode();

        /// <summary>
        /// Apply this drawable to the draw node
        /// </summary>
        /// <param name="node">The node to apply properties to</param>
        protected virtual void ApplyDrawNode(DrawNode node)
        {
        }

        protected virtual void OnWorldMatrixInvalidated()
        {
        }

        /// <summary>
        /// Invalidates this subtree's world matrices
        /// </summary>
        protected virtual void InvalidateParentPosition()
        {
            worldMatrix.Invalidate();
            OnWorldMatrixInvalidated();

            foreach(var child in children)
            {
                child.InvalidateParentPosition();
            }
        }

        protected virtual void RefreshToWorldMatrix()
        {
            bool applyPivoting = false;
            if(rotationPivot != Vector3.Zero)
                applyPivoting = true;

            if(applyPivoting)
                toWorldMatrix = Matrix4.CreateTranslation(-rotationPivot);
            else
                toWorldMatrix = Matrix4.Identity;

            toWorldMatrix = Matrix4.CreateScale(scale) * Matrix4.CreateFromQuaternion(rotation) * toWorldMatrix;

            if(applyPivoting)
                toWorldMatrix = Matrix4.CreateTranslation(rotationPivot + position) * toWorldMatrix;
            else
                toWorldMatrix = Matrix4.CreateTranslation(position) * toWorldMatrix;
        }

        /// <summary>
        /// Invalidates the toWorld matrix, thus also invalidating the child drawables and their world matrices
        /// </summary>
        protected virtual void InvalidateToWorldMatrix()
        {
            isToWorldValid = false;
            InvalidateParentPosition();
        }
        
        protected internal virtual void PerformLoad(BaseGame game)
        {
            switch(LoadState)
            {
                case LoadState.Loaded:
                case LoadState.Alive:
                    return;
                case LoadState.Loading:
                    //loading on another thread
                    while(!IsLoaded) Thread.Sleep(1);
                    return;
                case LoadState.NotLoaded:
                    LoadState = LoadState.Loading;
                    break;
            }

            double t1 = Perf.CurrentTime;
            game.Dependencies.Initialize(this);
            double elapsed = Perf.CurrentTime - t1;
            if(elapsed > 50 && ThreadSafety.IsUpdateThread)
                Logger.Log($@"Drawable [{ToString()}] took {elapsed:0.00}ms to load and was not async!", LoggingTarget.Performance);
            LoadState = LoadState.Loaded;
        }

        /// <summary>
        /// Play initial animation etc.
        /// </summary>
        protected virtual void LoadComplete() { }

        protected void UpdateTransformsOfType(Type specificType)
        {
            //For simplicity let's just update *all* transforms.
            //The commented (more optimised code) below doesn't consider past "removed" transforms, which can cause discrepancies.
            UpdateTransforms();

            //foreach (ITransform t in Transforms.AliveItems)
            //    if (t.GetType() == specificType)
            //        t.Apply(this);
        }

        /// <summary>
        /// Runs once on the update thread after loading has finished.
        /// </summary>
        private bool loadComplete()
        {
            if(LoadState < LoadState.Loaded) return false;

            mainThread = Thread.CurrentThread;

            scheduler?.SetCurrentThread(mainThread);

            LifetimeStart = Time.Current;
            LoadState = LoadState.Alive;
            LoadComplete();
            return true;
        }

        /// <summary>
        /// Process updates to this drawable based on loaded transforms.
        /// </summary>
        /// <returns>Whether we should draw this drawable.</returns>
        private void UpdateTransforms()
        {
            if(transforms == null || transforms.Count == 0) return;

            transforms.Update(Time);

            foreach(ITransform t in transforms.AliveItems)
                t.Apply(this);
        }

        private void transforms_OnRemoved(ITransform t)
        {
            t.Apply(this); //make sure we apply one last time.
        }
    }
}