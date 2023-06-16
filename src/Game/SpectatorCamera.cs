using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using VoxelGame.Engine.Rendering;
using VoxelGame.Framework;
using VoxelGame.Framework.Helpers;

namespace VoxelGame.Game
{
    public class SpectatorCamera : BaseCamera
    {
        public float Yaw;
        public float Pitch;
        public Vector3 Translation;

        // Vector to multiply by to discard the y axis.
        private static readonly Vector3 discardY = new Vector3(1, 0, 1);

        // 90 degrees in radians.
        private const float MAX_PITCH = 1.5707f;
        private const float MIN_PITCH = -1.5707f;

        // In half a deg per second per mouse unit.
        public const float SENSITIVITY = 0.4f;
        private const float TURNSPEED = SENSITIVITY / 720 * MathF.PI;

        private const float SPEED = 5f;
        private const float SPRINT_MULTIPLIER = 3f;

        public SpectatorCamera(Vector2i viewport, float fov, Vector3 translation, float yaw, float pitch) :
            base(viewport, fov, BuildTranformMatrix(pitch, yaw, translation))
        {
            Translation = translation;
            Yaw = yaw;
            Pitch = pitch;
        }

        public override void Update(double deltaTime)
        {
            // Get forward and right movement axes.
            Vector3 forward = _transformMatrix.GetForwardRaw() * discardY;
            forward.NormalizeFast();
            Vector3 right = _transformMatrix.GetRightRaw() * discardY;
            right.NormalizeFast();

            // Build input vector from keyboard input.
            Vector3 input = Vector3.Zero;

            if (Input.IsKeyPressed(Keys.W)) input += forward;
            if (Input.IsKeyPressed(Keys.S)) input -= forward;
            if (Input.IsKeyPressed(Keys.A)) input += right;
            if (Input.IsKeyPressed(Keys.D)) input -= right;

            if (Input.IsKeyPressed(Keys.Space)) input.Y += 1;
            if (Input.IsKeyPressed(Keys.LeftControl)) input.Y -= 1;

            // For some reason the default Normalize makes the vector NaN here.
            input.NormalizeFast();

            // Shift key makes the camera fly faster.
            if (Input.IsKeyPressed(Keys.LeftShift)) input *= SPRINT_MULTIPLIER;

            input *= SPEED * (float)deltaTime;
            Translation += input;

            // Looking around with the mouse.
            // TODO: Are pitch and yaw inverted here?
            float yawDelta = Input.MouseDelta.X * TURNSPEED;
            Yaw = MathHelper.NormalizeRadians(Yaw - yawDelta);
            float pitchDelta = Input.MouseDelta.Y * TURNSPEED;
            Pitch = MathHelper.Clamp(Pitch + pitchDelta, MIN_PITCH, MAX_PITCH);

            _transformMatrix = BuildTranformMatrix(Pitch, Yaw, Translation);
            _dirtyFlags |= DirtyFlags.ViewMatrix;

            base.Update(deltaTime);
        }

        // Since the matrix should never be scaled we don't need to normalize these.
        /// <summary>
        /// The forward vector of the cameras transform matrix.
        /// </summary>
        public Vector3 Forward => _transformMatrix.GetForwardRaw();
        /// <summary>
        /// The right vector of the cameras transform matrix.
        /// </summary>
        public Vector3 Right => _transformMatrix.GetRightRaw();
        /// <summary>
        /// The up vector of the cameras transform matrix.
        /// </summary>
        public Vector3 Up => _transformMatrix.GetUpRaw();

        private static Matrix4 BuildTranformMatrix(float pitch, float yaw, Vector3 translation)
        {
            return Matrix4.CreateRotationX(pitch)
                * Matrix4.CreateRotationY(yaw)
                * Matrix4.CreateTranslation(translation);
        }
    }
}
