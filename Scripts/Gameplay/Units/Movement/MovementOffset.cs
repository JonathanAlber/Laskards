using System;

namespace Gameplay.Units.Movement
{
    /// <summary>
    /// Immutable offset in logical board space.
    /// X is horizontal (file), Y is vertical (rank / forward).
    /// Positive Y is considered the "forward" direction for the owning team.
    /// </summary>
    [Serializable]
    public struct MovementOffset
    {
        /// <summary>
        /// Horizontal delta in tiles.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Vertical delta in tiles.
        /// </summary>
        public int Y { get; }

        public MovementOffset(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}