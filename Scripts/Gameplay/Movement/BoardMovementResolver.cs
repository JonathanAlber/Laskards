using System.Collections.Generic;
using Gameplay.Board;
using Gameplay.CardExecution;
using Gameplay.Units;
using Gameplay.Units.Movement;
using Utility.Logging;

namespace Gameplay.Movement
{
    /// <summary>
    /// Pure rules engine that turns a unit's movement definition and current board state
    /// into a set of legal destination tiles.
    /// </summary>
    public static class BoardMovementResolver
    {
        /// <summary>
        /// Computes legal destination tiles for the given unit.
        /// </summary>
        /// <param name="board">Board instance.</param>
        /// <param name="unit">Unit to move.</param>
        /// <param name="definition">Unit movement definition.</param>
        /// <param name="rows">Board rows.</param>
        /// <param name="cols">Board columns.</param>
        /// <returns>List of legal destination tiles.</returns>
        public static List<Tile> GetLegalMoves(GameBoard board, UnitController unit,
            UnitMovementDefinition definition, int rows, int cols)
        {
            List<Tile> result = new();

            Tile origin = unit.CurrentTile;
            if (origin == null)
            {
                CustomLogger.LogWarning($"Unit {unit.name} is not on any tile, cannot compute legal moves.", null);
                return result;
            }

            int forwardSign = unit.Team == ETeam.Player ? 1 : -1;

            foreach (UnitMovementRule rule in definition.Rules)
            {
                int dirX = rule.Direction.X;
                int dirY = rule.Direction.Y * forwardSign;

                if (rule.IsJump)
                {
                    TryAddJump(board, unit, origin, dirX, dirY, rule, rows, cols, result);
                    continue;
                }

                TryAddSlide(board, unit, origin, dirX, dirY, rule, rows, cols, result);
            }

            return result;
        }

        private static void TryAddJump(GameBoard board, UnitController unit, Tile origin, int dirX, int dirY,
            UnitMovementRule rule, int rows, int cols, List<Tile> result)
        {
            int targetRow = origin.Row + dirY;
            int targetCol = origin.Column + dirX;

            if (!IsInside(targetRow, targetCol, rows, cols))
                return;

            Tile tile = board.GetTile(targetRow, targetCol);
            if (tile == null)
            {
                CustomLogger.LogWarning($"Tile at ({targetRow}, {targetCol}) is null, cannot add jump move.", null);
                return;
            }

            if (!tile.IsOccupied() && rule.CanMoveToEmpty)
            {
                result.Add(tile);
                return;
            }

            UnitController occupant = tile.OccupyingUnit;
            if (occupant == null)
                return;

            if (occupant.Team == unit.Team)
                return;

            if (!occupant.CanBeAttacked())
                return;

            if (rule.CanCapture)
                result.Add(tile);
        }

        private static void TryAddSlide(GameBoard board, UnitController unit, Tile origin, int dirX, int dirY,
            UnitMovementRule rule, int rows, int cols, List<Tile> result)
        {
            for (int step = 1; step <= rule.MaxSteps; step++)
            {
                int r = origin.Row + dirY * step;
                int c = origin.Column + dirX * step;

                if (!IsInside(r, c, rows, cols))
                    return;

                Tile tile = board.GetTile(r, c);
                if (tile == null)
                {
                    CustomLogger.LogWarning($"Tile at ({r}, {c}) is null, cannot add slide move.", null);
                    return;
                }

                if (!tile.IsOccupied())
                {
                    if (rule.CanMoveToEmpty)
                        result.Add(tile);

                    continue;
                }

                UnitController occupant = tile.OccupyingUnit;
                if (occupant == null)
                    return;

                if (occupant.Team == unit.Team)
                    return;

                if (!occupant.CanBeAttacked())
                    return;

                if (!rule.CanCapture)
                    return;

                result.Add(tile);
                return;
            }
        }

        /// <summary>
        /// Checks if the given row and column are inside the board boundaries.
        /// </summary>
        private static bool IsInside(int row, int col, int rows, int cols)
        {
            if (row < 0 || row >= rows)
                return false;

            return col >= 0 && col < cols;
        }
    }
}