using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Engine.Rendering;
using VoxelGame.Framework;
using VoxelGame.Framework.Helpers;

namespace VoxelGame.Game
{
    /// <summary>
    /// Camera extension to make a flying camera controller that can be transformed into an fps controller easily.
    /// </summary>
    public class FlyingCamera : BaseCamera
    {
        /// <summary>
        /// Yaw of the camera.
        /// </summary>
        public float Yaw;
        /// <summary>
        /// Pitch of the camera.
        /// </summary>
        public float Pitch;
        /// <summary>
        /// The translation of the cameras transform matrix.
        /// </summary>
        public Vector3 Translation;

        private static readonly Vector3 discardY = new Vector3(1, 0, 1);

        // 90 degrees in radians.
        private const float MAX_PITCH = 1.5707f;
        private const float MIN_PITCH = -1.5707f;

        // In half a deg per second per mouse unit.
        public const float SENSITIVITY = 0.4f;
        private const float TURNSPEED = SENSITIVITY / 720 * MathF.PI;
        private const float SPEED = 5f;

        public FlyingCamera(Vector2i viewport, float fov, Vector3 translation, float yaw, float pitch) :
            base(viewport, fov, BuildTranformMatrix(pitch, yaw, translation))
        {
            Translation = translation;
            Yaw = yaw;
            Pitch = pitch;
        }

        public override void Update(double deltaTime)
        {
            // Update movement axis
            Vector3 forward = _transformMatrix.GetForwardRaw() * discardY;
            forward.NormalizeFast();
            Vector3 right = _transformMatrix.GetRightRaw() * discardY;
            right.NormalizeFast();

            // Build input vector from keyboard input.
            // For some reason input needs to be inverted.
            Vector3 input = Vector3.Zero;
            if (Input.IsKeyPressed(Keys.W)) input += forward;
            else if (Input.IsKeyPressed(Keys.S)) input -= forward;
            if (Input.IsKeyPressed(Keys.A)) input += right;
            else if (Input.IsKeyPressed(Keys.D)) input -= right;
            if (Input.IsKeyPressed(Keys.Space)) input.Y += 1;
            else if (Input.IsKeyPressed(Keys.LeftControl)) input.Y -= 1;

            // For some reason the default Normalize makes the vector NaN.
            input.NormalizeFast();
            // Sprint key
            if (Input.IsKeyPressed(Keys.LeftShift)) input *= 3;
            input *= SPEED * (float)deltaTime;
            Translation += input;

            // Looking around with the mouse.
            // Again, pitch and yaw need to be inverted.
            float yawDelta = Input.MouseDelta.X * TURNSPEED;
            float pitchDelta = Input.MouseDelta.Y * TURNSPEED;
            Yaw = MathHelper.NormalizeRadians(Yaw - yawDelta);
            Pitch = MathHelper.Clamp(Pitch + pitchDelta, MIN_PITCH, MAX_PITCH);

            _transformMatrix = BuildTranformMatrix(Pitch, Yaw, Translation);
            _dirtyFlags |= DirtyFlags.ViewMatrix;

            base.Update(deltaTime);
        }

        // Since the matrix should never be scaled we dont neeed to normalize these.
        #region Vector Properties
        /// <summary>
        /// The unnormalized forward vector of the cameras transform matrix.
        /// </summary>
        public Vector3 Forward => _transformMatrix.GetForwardRaw();
        /// <summary>
        /// The unnormalized right vector of the cameras transform matrix.
        /// </summary>
        public Vector3 Right => _transformMatrix.GetRightRaw();
        /// <summary>
        /// The unnormalized up vector of the cameras transform matrix.
        /// </summary>
        public Vector3 Up => _transformMatrix.GetUpRaw();
        #endregion

        private static Matrix4 BuildTranformMatrix(float pitch, float yaw, Vector3 translation)
        {
            return Matrix4.CreateRotationX(pitch)
                * Matrix4.CreateRotationY(yaw)
                * Matrix4.CreateTranslation(translation);
        }
    }
}
