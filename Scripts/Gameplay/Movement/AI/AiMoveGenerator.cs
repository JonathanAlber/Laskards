using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Units.Movement;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Generates legal AI moves from a pure AI snapshot state.
    /// </summary>
    public class AiMoveGenerator
    {
        private const float ForwardDeltaHeuristicMultiplier = 0.1f;

        private readonly AiUnitValueCalculator _unitValueCalculator;

        public AiMoveGenerator(AiUnitValueCalculator aiUnitValueCalculator)
        {
            _unitValueCalculator = aiUnitValueCalculator;
        }

        /// <summary>
        /// Computes all legal moves for the given team in the specified AI state.
        /// </summary>
        public List<AiMove> GenerateMoves(AiGameState state, ETeam team)
        {
            List<AiMove> moves = new();

            IReadOnlyList<AiUnitSnapshot> units = state.Units;

            foreach (AiUnitSnapshot snapshot in units)
            {
                if (!snapshot.IsAlive)
                    continue;

                if (snapshot.Team != team)
                    continue;

                if (snapshot.MovesLeft <= 0)
                    continue;

                UnitMovementDefinition definition = StandardUnitMovementLibrary.GetDefinition(snapshot.UnitType);
                if (definition == null)
                    continue;

                GenerateMovesForUnit(state, snapshot, definition, moves);
            }

            // Sort moves by expected quality
            moves.Sort((a, b) =>
            {
                // 1. Captures
                int cap = b.IsCapture.CompareTo(a.IsCapture);
                if (cap != 0)
                    return cap;

                // 2. Forward movement
                int fwd = b.ForwardDelta.CompareTo(a.ForwardDelta);
                if (fwd != 0)
                    return fwd;

                // 3. Heuristic delta (MVV-LVA & forward bonus)
                return b.HeuristicDelta.CompareTo(a.HeuristicDelta);
            });

            return moves;
        }

        private void GenerateMovesForUnit(AiGameState state, AiUnitSnapshot unit, UnitMovementDefinition definition,
            List<AiMove> result)
        {
            int forwardSign = unit.Team == ETeam.Player ? 1 : -1;

            foreach (UnitMovementRule rule in definition.Rules)
            {
                int dirX = rule.Direction.X;
                int dirY = rule.Direction.Y * forwardSign;

                if (rule.IsJump)
                    TryAddJump(state, unit, dirX, dirY, rule, result);
                else
                    TryAddSlide(state, unit, dirX, dirY, rule, result);
            }
        }

        private void TryAddJump(AiGameState state, AiUnitSnapshot unit, int dirX, int dirY, UnitMovementRule rule,
            List<AiMove> result)
        {
            int targetRow = unit.Row + dirY;
            int targetColumn = unit.Column + dirX;

            if (!IsInside(targetRow, targetColumn, state.Rows, state.Columns))
                return;

            if (state.IsTileBlocked(targetRow, targetColumn))
                return;

            AiUnitSnapshot occupant = state.UnitAt[targetColumn, targetRow];

            if (occupant == null)
            {
                if (rule.CanMoveToEmpty)
                    result.Add(new AiMove(unit.Id, targetRow, targetColumn));
                return;
            }

            if (occupant.Team == unit.Team)
                return;

            if (!occupant.CanBeAttacked)
                return;

            if (rule.CanCapture)
                result.Add(BuildMoveWithHeuristics(unit, targetRow, targetColumn, occupant));
        }

        private void TryAddSlide(AiGameState state, AiUnitSnapshot unit, int dirX, int dirY, UnitMovementRule rule,
            List<AiMove> result)
        {
            for (int step = 1; step <= rule.MaxSteps; step++)
            {
                int targetRow = unit.Row + dirY * step;
                int targetColumn = unit.Column + dirX * step;

                if (!IsInside(targetRow, targetColumn, state.Rows, state.Columns))
                    return;

                if (state.IsTileBlocked(targetRow, targetColumn))
                    return;

                AiUnitSnapshot occupant = state.UnitAt[targetColumn, targetRow];
                if (occupant == null)
                {
                    if (rule.CanMoveToEmpty)
                        result.Add(new AiMove(unit.Id, targetRow, targetColumn));
                    continue;
                }

                if (occupant.Team == unit.Team)
                    return;

                if (!occupant.CanBeAttacked)
                    return;

                if (rule.CanCapture)
                    result.Add(BuildMoveWithHeuristics(unit, targetRow, targetColumn, occupant));

                return;
            }
        }

        private AiMove BuildMoveWithHeuristics(AiUnitSnapshot unit, int targetRow, int targetColumn,
            AiUnitSnapshot occupant)
        {
            if (occupant == null)
                return new AiMove(unit.Id, targetRow, targetColumn);

            bool isCapture = occupant.Team != unit.Team;

            // Boss moves upward, Player moves downward
            int forwardDelta = unit.Team == ETeam.Boss
                ? unit.Row - targetRow
                : targetRow - unit.Row;

            // Heuristic delta: improved approximation of move quality
            float heuristicDelta = 0f;

            if (isCapture)
            {
                // MVV-LVA style: prefer taking high-value units with low-value units
                float victimValue = _unitValueCalculator.Compute(occupant);
                float attackerValue = _unitValueCalculator.Compute(unit);
                heuristicDelta += victimValue * 2f - attackerValue;
            }

            // Forward progress small bonus / backwards slight penalty
            heuristicDelta += forwardDelta * ForwardDeltaHeuristicMultiplier;

            return new AiMove(unit.Id, targetRow, targetColumn, isCapture, forwardDelta, heuristicDelta);
        }

        private static bool IsInside(int row, int column, int rows, int columns)
        {
            if (row < 0 || row >= rows)
                return false;

            return column >= 0 && column < columns;
        }
    }
}