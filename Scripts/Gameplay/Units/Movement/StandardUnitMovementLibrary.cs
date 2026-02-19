using System.Collections.Generic;

namespace Gameplay.Units.Movement
{
    /// <summary>
    /// Provides standard, immutable chess-like movement definitions for unit types A–E.
    /// These definitions can be shared across all units of the same type.
    /// </summary>
    public static class StandardUnitMovementLibrary
    {
        private static readonly UnitMovementDefinition TypeAPawn;
        private static readonly UnitMovementDefinition TypeBKnight;
        private static readonly UnitMovementDefinition TypeCBishop;
        private static readonly UnitMovementDefinition TypeDRook;
        private static readonly UnitMovementDefinition TypeEQueen;

        static StandardUnitMovementLibrary()
        {
            TypeAPawn = CreatePawnDefinition();
            TypeBKnight = CreateKnightDefinition();
            TypeCBishop = CreateBishopDefinition();
            TypeDRook = CreateRookDefinition();
            TypeEQueen = CreateQueenDefinition();
        }

        /// <summary>
        /// Returns the standard movement definition for a given unit type.
        /// </summary>
        /// <param name="unitType">Logical unit type (A–E).</param>
        /// <returns>The corresponding movement definition.</returns>
        public static UnitMovementDefinition GetDefinition(EUnitType unitType)
        {
            return unitType switch
            {
                EUnitType.A => TypeAPawn,
                EUnitType.B => TypeBKnight,
                EUnitType.C => TypeCBishop,
                EUnitType.D => TypeDRook,
                EUnitType.E => TypeEQueen,
                _ => TypeAPawn
            };
        }

        /// <summary>
        /// Creates the movement definition for a pawn.
        /// Moves one tile forward into an empty space, or one tile diagonally forward when capturing an opponent.
        /// No double moves or en passant. Forward direction is positive Y; invert for the opposing team.
        /// </summary>
        private static UnitMovementDefinition CreatePawnDefinition()
        {
            List<UnitMovementRule> rules = new()
            {
                new UnitMovementRule(new MovementOffset(0, 1), 1, false, true, false),
                new UnitMovementRule(new MovementOffset(-1, 1), 1, false, false, true),
                new UnitMovementRule(new MovementOffset(1, 1), 1, false, false, true)
            };

            return new UnitMovementDefinition(rules);
        }

        /// <summary>
        /// Creates the movement definition for a knight.
        /// Uses standard chess L-shaped jumps that ignore blockers.
        /// </summary>
        private static UnitMovementDefinition CreateKnightDefinition()
        {
            List<UnitMovementRule> rules = new()
            {
                CreateKnightRule(1, 2), CreateKnightRule(2, 1),
                CreateKnightRule(-1, 2), CreateKnightRule(-2, 1),
                CreateKnightRule(1, -2), CreateKnightRule(2, -1),
                CreateKnightRule(-1, -2), CreateKnightRule(-2, -1)
            };

            return new UnitMovementDefinition(rules);
        }

        /// <summary>
        /// Creates the movement definition for a bishop.
        /// Slides diagonally any number of tiles until blocked.
        /// </summary>
        private static UnitMovementDefinition CreateBishopDefinition()
        {
            const int maxBoardDistance = 8;
            List<UnitMovementRule> rules = new()
            {
                new UnitMovementRule(new MovementOffset(1, 1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(-1, 1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(1, -1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(-1, -1), maxBoardDistance, false, true, true)
            };

            return new UnitMovementDefinition(rules);
        }

        /// <summary>
        /// Creates the movement definition for a rook.
        /// Slides vertically or horizontally any number of tiles until blocked.
        /// </summary>
        private static UnitMovementDefinition CreateRookDefinition()
        {
            const int maxBoardDistance = 8;
            List<UnitMovementRule> rules = new()
            {
                new UnitMovementRule(new MovementOffset(0, 1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(0, -1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(1, 0), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(-1, 0), maxBoardDistance, false, true, true)
            };

            return new UnitMovementDefinition(rules);
        }

        /// <summary>
        /// Creates the movement definition for a queen.
        /// Combines bishop (diagonal) and rook (orthogonal) sliding movement.
        /// </summary>
        private static UnitMovementDefinition CreateQueenDefinition()
        {
            const int maxBoardDistance = 8;
            List<UnitMovementRule> rules = new()
            {
                // Diagonal movement (bishop-like)
                new UnitMovementRule(new MovementOffset(1, 1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(-1, 1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(1, -1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(-1, -1), maxBoardDistance, false, true, true),

                // Orthogonal movement (rook-like)
                new UnitMovementRule(new MovementOffset(0, 1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(0, -1), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(1, 0), maxBoardDistance, false, true, true),
                new UnitMovementRule(new MovementOffset(-1, 0), maxBoardDistance, false, true, true)
            };

            return new UnitMovementDefinition(rules);
        }

        /// <summary>
        /// Helper to create a standard knight move rule.
        /// </summary>
        private static UnitMovementRule CreateKnightRule(int x, int y)
        {
            return new UnitMovementRule(new MovementOffset(x, y), 1, true, true, true);
        }
    }
}