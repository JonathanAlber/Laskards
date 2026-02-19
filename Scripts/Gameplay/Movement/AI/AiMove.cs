using System;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Immutable representation of a single move in the search tree.
    /// </summary>
    public readonly struct AiMove : IEquatable<AiMove>
    {
        /// <summary>
        /// Identifier of the unit performing the move.
        /// </summary>
        public int UnitId { get; }

        /// <summary>
        /// Target row of the move.
        /// </summary>
        public int ToRow { get; }

        /// <summary>
        /// Target column of the move.
        /// </summary>
        public int ToColumn { get; }

        /// <summary>
        /// Indicates whether this is a pass move (no unit moves).
        /// </summary>
        public bool IsPass { get; }

        /// <summary>
        /// Indicates whether this move results in a capture.
        /// </summary>
        public bool IsCapture { get; }

        /// <summary>
        /// Forward movement delta achieved by this move.
        /// </summary>
        public int ForwardDelta { get; }

        /// <summary>
        /// Heuristic score delta achieved by this move.
        /// </summary>
        public float HeuristicDelta { get; }

        /// <summary>
        /// Creates a non-pass move for a unit.
        /// </summary>
        public AiMove(int unitId, int toRow, int toColumn, bool isCapture = false, int forwardDelta = 0,
            float heuristicDelta = 0)
        {
            UnitId = unitId;
            ToRow = toRow;
            ToColumn = toColumn;
            IsPass = false;
            IsCapture = isCapture;
            ForwardDelta = forwardDelta;
            HeuristicDelta = heuristicDelta;
        }

        /// <summary>
        /// Creates a pass move where no unit is moved.
        /// </summary>
        public static AiMove CreatePass() => new(-1, 0, 0, isPass: true);

        private AiMove(int unitId, int toRow, int toColumn, bool isPass)
        {
            UnitId = unitId;
            ToRow = toRow;
            ToColumn = toColumn;
            IsPass = isPass;
            IsCapture = false;
            ForwardDelta = 0;
            HeuristicDelta = 0f;
        }

        public bool Equals(AiMove other) => UnitId == other.UnitId && ToRow == other.ToRow
                                                        && ToColumn == other.ToColumn && IsPass == other.IsPass;

        public override bool Equals(object obj) => obj is AiMove other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(UnitId, ToRow, ToColumn, IsPass);
    }
}