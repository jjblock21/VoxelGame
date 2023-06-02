using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelGame.Framework.Helpers;

namespace VoxelGame.Engine.Rendering
{
    /// <summary>
    /// Base class, handling the transform, view and projection matrices.<br/>
    /// Designed to be extended to implement behavior.
    /// </summary>
    public class BaseCamera
    {
        protected Matrix4 _transformMatrix;

        private Matrix4 _projMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _projViewMat;

        protected const float NEAR_CLIPPING_PLANE = 0.1f;
        protected const float FAR_CLIPPING_PLANE = 1000f;

        protected float _aspectRatio;
        protected Vector2i _viewport;
        private float _fov;

        protected DirtyFlags _dirtyFlags;

        public BaseCamera(Vector2i viewport, float fov, Matrix4 transform)
        {
            _viewport = viewport;
            _fov = MathHelper.DegreesToRadians(fov);

            _transformMatrix = transform;
            // Update everything.
            _dirtyFlags = (DirtyFlags)0b1111;
        }

        public virtual void Update(double deltaTime)
        {
            // Update the view matrix if the transform matrix changed.
            if ((_dirtyFlags & DirtyFlags.ViewMatrix) != 0)
            {
                Matrix4 copy = _transformMatrix;
                // Lens faces backward, so forward and right need to be inverted.
                copy.Row0.Xyz = -copy.Row0.Xyz;
                copy.Row2.Xyz = -copy.Row2.Xyz;
                _viewMatrix = Matrix4.Invert(copy);

                _dirtyFlags &= ~DirtyFlags.ViewMatrix;
                // Also update the combined view and proj matrix.
                _dirtyFlags |= DirtyFlags.ProjViewMatrix;
            }

            // Update the GL view port and aspect ratio if the view port has changed.
            if ((_dirtyFlags & DirtyFlags.Viewport) != 0)
            {
                // Update view port and aspect ratio.
                GL.Viewport(0, 0, _viewport.X, _viewport.Y);
                _aspectRatio = _viewport.X / (float)_viewport.Y;

                _dirtyFlags &= ~DirtyFlags.Viewport;
                // Also update the projection matrix.
                _dirtyFlags |= DirtyFlags.ProjMatrix;
            }

            // Update the projection matrix if aspect ration or fov changed.
            if ((_dirtyFlags & DirtyFlags.ProjMatrix) != 0)
            {
                // Recalculate projection matrix.
                _projMatrix = Matrix4.CreatePerspectiveFieldOfView(_fov, _aspectRatio, NEAR_CLIPPING_PLANE, FAR_CLIPPING_PLANE);

                _dirtyFlags &= ~DirtyFlags.ProjMatrix;
                // Also update the combined view and proj matrix.
                _dirtyFlags |= DirtyFlags.ProjViewMatrix;
            }

            // Update the combined view and proj matrix if the view or projection matrix changed.
            if ((_dirtyFlags & DirtyFlags.ProjViewMatrix) != 0)
            {
                // Multiply projection and view matrix together.
                _projViewMat = _viewMatrix * _projMatrix;
                _dirtyFlags &= ~DirtyFlags.ProjViewMatrix;
            }
        }

        public Matrix4 ProjViewMat { get => _projViewMat; }
        public Matrix4 ViewMat { get => _viewMatrix; }
        public Matrix4 ProjMat { get => _projMatrix; }

        public Matrix4 TransformMat
        {
            get => _transformMatrix;
            set
            {
                if (_transformMatrix == value) return;
                _transformMatrix = value;
                _dirtyFlags |= DirtyFlags.ViewMatrix;
            }
        }

        /// <summary>
        /// Changes the view port of the camera. (Does not need to be called from within a GL context)
        /// </summary>
        public void ChangeViewport(Vector2i viewport)
        {
            _viewport = viewport;
            _dirtyFlags |= DirtyFlags.Viewport;
        }

        /// <summary>
        /// The fov on the y axis of the view port in radians.
        /// </summary>
        public float Fov
        {
            get => _fov;
            set
            {
                _fov = MathHelper.NormalizeRadians(value);
                _dirtyFlags |= DirtyFlags.ProjMatrix;
            }
        }

        /// <summary>
        /// The fov on the y axis of the view port in degrees.
        /// </summary>
        public float FovDeg
        {
            get => MathH.ToDeg(_fov);
            set { Fov = MathH.ToRad(value); }
        }

        protected enum DirtyFlags
        {
            ViewMatrix = 0b1,
            Viewport = 0b10,
            ProjMatrix = 0b100,
            ProjViewMatrix = 0b1000
        }
    }
}