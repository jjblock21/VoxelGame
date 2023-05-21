using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;
using VoxelGame.Engine.Voxels;
using VoxelGame.Framework.Helpers;
using VoxelGame.Game;
using VoxelGame.Game.Blocks;
using static VoxelGame.Framework.Helpers.MethodImplConstants;

namespace VoxelGame.Engine
{
    public static class Raycast
    {
        /// <summary>
        /// Casts a ray into the world using the Fast Voxel Traversal Algorithm.
        /// </summary>
        /// <param name="origin">Origin of the ray.</param>
        /// <param name="direction">Normalized direction of the ray.</param>
        /// <param name="length">Max distance to cast the ray.</param>
        /// <param name="result">Result struct, contianing information about the hit or <see langword="default"/></param>
        /// <returns><see langword="true"/> if the ray hit a block that is not <see cref="BlockType.Air"/></returns>
        /// <exception cref="ArgumentException"/>
        [MethodImpl(OPTIMIZE)]
        public static bool Perform(Vector3 origin, Vector3 direction, float length, out Result result)
        {
            if (direction == Vector3.Zero)
                throw new ArgumentException("Direction can't be zero!");

            Result res = default;
            sbyte state = 0;

            // The cell the ray is currently in.
            Vector3i cell = VectorUtility.FloorVec3(origin);
            World world = Minecraft.Instance.CurrentWorld!;

            // Wether to step forward or backward into next cell for each case.
            int stepX = MathF.Sign(direction.X);
            int stepY = MathF.Sign(direction.Y);
            int stepZ = MathF.Sign(direction.Z);

            // Length required to be added to the ray in order to cross over to the next int value per case.
            float nextFaceX = IntBnd(origin.X, direction.X);
            float nextFaceY = IntBnd(origin.Y, direction.Y);
            float nextFaceZ = IntBnd(origin.Z, direction.Z);

            // Rescale length by the length of the direction vector.
            length *= MathHelper.InverseSqrtFast(direction.LengthSquared);

            // The change in the length of the ray relative to the scaled length per case.
            float lengthDeltaX = stepX / direction.X;
            float lengthDeltaY = stepY / direction.Y;
            float lengthDeltaZ = stepZ / direction.Z;

            [MethodImpl(INLINE)]
            void exitIfHit(uint enterDirection)
            {
                if (world.TryGetBlock(cell, out BlockType block) && block != BlockType.Air)
                {
                    // Exit and return success and the result.
                    state = 1;
                    res = new Result(block, cell, enterDirection);
                }
            }

            [MethodImpl(INLINE)]
            void advanceRayX()
            {
                // Stop the ray if the length has been exceeded and return error.
                if (nextFaceX > length) state = -1;

                cell.X += stepX;
                nextFaceX += lengthDeltaX;
                exitIfHit(stepX < 0 ? 1u : 3u);
            }

            [MethodImpl(INLINE)]
            void advanceRayY()
            {
                // Stop the ray if the length has been exceeded.
                if (nextFaceY > length) state = -1;

                cell.Y += stepY;
                nextFaceY += lengthDeltaY;
                exitIfHit(stepY < 0 ? 4u : 5u);
            }

            [MethodImpl(INLINE)]
            void advanceRayZ()
            {
                // Stop the ray if the length has been exceeded.
                if (nextFaceZ > length) state = -1;

                cell.Z += stepZ;
                nextFaceZ += lengthDeltaZ;
                exitIfHit(stepZ < 0 ? 0u : 2u);
            }

            while (state == 0)
            {
                // Find smallest distance and advance ray by the corrensponding case.
                if (nextFaceX < nextFaceY)
                {
                    if (nextFaceX < nextFaceZ) advanceRayX();
                    else advanceRayZ();
                }
                else
                {
                    if (nextFaceY < nextFaceZ) advanceRayY();
                    else advanceRayZ();
                }
            }

            result = res;
            return state > 0;
        }

        /// <summary>
        /// Calculates the amount to add to the ray to cross over into the next cell.
        /// </summary>
        [MethodImpl(OPTIMIZE | INLINE)]
        private static float IntBnd(float left, float right)
        {
            // If right is negative and left is an integer return 0. (edge case)
            if (right < 0 && left - (int)left == 0) return 0;

            float a;
            if (right <= 0) a = left - MathF.Floor(left);
            //             \/ Another edge case
            else a = left == 0 ? 1 : MathF.Ceiling(left) - left;

            return a / MathF.Abs(right);
        }

        private class RaycastInfo
        {

        }

        public struct Result
        {
            /// <summary>
            /// The block which was hit.
            /// </summary>
            public readonly BlockType Type;

            /// <summary>
            /// The location of the block which was hit.
            /// </summary>
            public readonly Vector3i Location;

            /// <summary>
            /// The direction of the face which was hit as a directon integer.
            /// </summary>
            public readonly uint FaceDirection;

            public Result(BlockType type, Vector3i location, uint direction)
            {
                Type = type;
                Location = location;
                FaceDirection = direction;
            }
        }
    }
}
