using System;
using Utility.Logging;

namespace Gameplay.Units.Movement
{
    /// <summary>
    /// Describes a single directional movement rule for a unit.
    /// For example, one diagonal ray of a bishop or one knight jump.
    /// </summary>
    [Serializable]
    public sealed class UnitMovementRule
    {
        /// <summary>
        /// Base direction in local board space (before team orientation is applied).
        /// </summary>
        public MovementOffset Direction { get; }

        /// <summary>
        /// Maximum number of tiles this rule can move along <see cref="Direction"/>.
        /// Use 1 for step / jump moves (pawn, king, knight) and larger values for sliding pieces (bishop, rook, queen).
        /// </summary>
        public int MaxSteps { get; }

        /// <summary>
        /// Indicates whether this movement ignores intermediate blockers.
        /// True for knight-like jumps, false for sliding / stepping moves that stop at blockers.
        /// </summary>
        public bool IsJump { get; }

        /// <summary>
        /// Indicates whether this rule can move into an empty tile.
        /// </summary>
        public bool CanMoveToEmpty { get; }

        /// <summary>
        /// Indicates whether this rule can move into an occupied tile to perform a capture.
        /// </summary>
        public bool CanCapture { get; }

        /// <summary>
        /// Creates a new movement rule.
        /// </summary>
        /// <param name="direction">Base direction this rule moves in.</param>
        /// <param name="maxSteps">Maximum steps along the direction. Must be >= 1.</param>
        /// <param name="isJump">If true, ignores intermediate blockers.</param>
        /// <param name="canMoveToEmpty">If true, can move into empty tiles.</param>
        /// <param name="canCapture">If true, can move into occupied tiles for capture.</param>
        public UnitMovementRule(MovementOffset direction, int maxSteps, bool isJump, bool canMoveToEmpty, bool canCapture)
        {
            if (maxSteps < 1)
            {
                CustomLogger.LogWarning($"UnitMovementRule created with invalid maxSteps {maxSteps}. " +
                                        "Clamping to 1.", null);
                maxSteps = 1;
            }

            Direction = direction;
            MaxSteps = maxSteps;
            IsJump = isJump;
            CanMoveToEmpty = canMoveToEmpty;
            CanCapture = canCapture;
        }
    }
}