// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Caching;
using OpenTK;

namespace osu.Framework.Graphics3D
{
    public class Camera : Drawable3D
    {
        private Cached<Matrix4> projectionMatrix = new Cached<Matrix4>();
        private Cached<Matrix4> viewMatrix = new Cached<Matrix4>();
        private Cached<float> aspectRatio = new Cached<float>();
        private float fieldOfView = MathHelper.DegreesToRadians(60.0f);
        private Vector2 viewportSize = new Vector2(1280.0f, 720.0f);
        private float nearClipping = 0.1f;
        private float farClipping = 100.0f;
        
        /// <summary>
        /// Generates null, no need to draw the camera
        /// </summary>
        /// <returns></returns>
        protected override DrawNode3D CreateDrawNode() => null;

        public Camera()
        {
            aspectRatio.Refresh(() => viewportSize.X / viewportSize.Y);
            projectionMatrix.Refresh(RefreshProjectionMatrix);
        }

        /// <summary>
        /// The Camera- or View matrix
        /// </summary>
        public Matrix4 ViewMatrix => viewMatrix.EnsureValid() ? viewMatrix.Value :
            viewMatrix.Refresh(() =>
            {
                return WorldMatrix.Inverted(); // TODO: Test this, might need manual calculation
            });

        /// <summary>
        /// Inverse view matrix
        /// </summary>
        public Matrix4 InverseViewMatrix => WorldMatrix;

        /// <summary>
        /// The projection matrix for this camera
        /// </summary>
        public Matrix4 ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        /// <summary>
        /// The field of view of this camera
        /// </summary>
        public float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                if(value != fieldOfView)
                {
                    fieldOfView = value;
                    projectionMatrix.Invalidate();
                }
            }
        }

        /// <summary>
        /// The size of the viewport this camera renders to
        /// </summary>
        public Vector2 ViewportSize
        {
            get { return viewportSize; }
            set
            {
                if(value != viewportSize)
                {
                    viewportSize = value;
                    projectionMatrix.Invalidate();
                }
            }
        }
        
        public float AspectRatio
        {
            get { return aspectRatio; }
        }

        /// <summary>
        /// Distance of the near clipping plane
        /// </summary>
        public float NearClipping
        {
            get { return nearClipping; }
            set
            {
                nearClipping = value;
                projectionMatrix.Invalidate();
            }
        }

        /// <summary>
        /// Distance of the far clipping plane
        /// </summary>
        public float FarClipping
        {
            get { return farClipping; }
            set
            {
                farClipping = value;
                projectionMatrix.Invalidate();
            }
        }

        protected override void OnWorldMatrixInvalidated()
        {
            base.OnWorldMatrixInvalidated();
            viewMatrix.Invalidate();
        }

        protected override void OnAddedToScene()
        {
            base.OnAddedToScene();

            // Auto-activate first camera
            Scene.AddCamera(this);
        }

        protected override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();

            // Unset scene camera
            Scene.RemoveCamera(this);
        }

        private Matrix4 RefreshProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fieldOfView, AspectRatio, nearClipping, farClipping);
        }
    }
}