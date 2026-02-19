using Gameplay.CardExecution;
using Gameplay.Units.Movement;
using Utility.Collections;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Builds attack maps for AI units based on their movement capabilities.
    /// </summary>
    public static class AiAttackMapBuilder
    {
        /// <summary>
        /// Builds attack maps for player and boss units in the given AI game state.
        /// </summary>
        /// <param name="state">The AI game state.</param>
        /// <param name="playerAttackMap">The output player attack map.</param>
        /// <param name="bossAttackMap">The output boss attack map.</param>
        public static void BuildAttackMaps(AiGameState state, out FlattenedArray<int> playerAttackMap,
            out FlattenedArray<int> bossAttackMap)
        {
            int rows = state.Rows;
            int cols = state.Columns;

            playerAttackMap = new FlattenedArray<int>(cols, rows);
            bossAttackMap = new FlattenedArray<int>(cols, rows);

            foreach (AiUnitSnapshot unit in state.Units)
            {
                if (!unit.IsAlive)
                    continue;

                UnitMovementDefinition def = StandardUnitMovementLibrary.GetDefinition(unit.UnitType);
                if (def == null)
                    continue;

                int forwardSign = unit.Team == ETeam.Player ? 1 : -1;
                FlattenedArray<int> targetMap =
                    unit.Team == ETeam.Player ? playerAttackMap : bossAttackMap;

                foreach (UnitMovementRule rule in def.Rules)
                {
                    int dirX = rule.Direction.X;
                    int dirY = rule.Direction.Y * forwardSign;

                    if (rule.IsJump)
                        AddJumpThreat(state, unit, dirX, dirY, rule, targetMap);
                    else
                        AddSlideThreat(state, unit, dirX, dirY, rule, targetMap);
                }
            }
        }

        /// <summary>
        /// Checks if the given row and column are inside the bounds of the game state.
        /// </summary>
        /// <param name="aiGameState">The AI game state.</param>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns><c>true</c> if inside bounds; otherwise, <c>false</c>.</returns>
        public static bool Inside(AiGameState aiGameState, int row, int column)
        {
            return row >= 0 && row < aiGameState.Rows && column >= 0 && column < aiGameState.Columns;
        }

        private static void AddJumpThreat(AiGameState state, AiUnitSnapshot unit, int xDirection, int yDirection,
            UnitMovementRule rule, FlattenedArray<int> attackMap)
        {
            int tr = unit.Row + yDirection;
            int tc = unit.Column + xDirection;

            if (!Inside(state, tr, tc))
                return;

            if (!rule.CanCapture)
                return;

            attackMap[tc, tr]++;
        }

        private static void AddSlideThreat(AiGameState state, AiUnitSnapshot unit, int dx, int dy,
            UnitMovementRule rule, FlattenedArray<int> attackMap)
        {
            for (int s = 1; s <= rule.MaxSteps; s++)
            {
                int tr = unit.Row + dy * s;
                int tc = unit.Column + dx * s;

                if (!Inside(state, tr, tc))
                    return;

                AiUnitSnapshot occ = state.UnitAt[tc, tr];

                if (occ == null)
                    continue;

                if (occ.Team == unit.Team)
                    return;

                if (rule.CanCapture)
                    attackMap[tc, tr]++;

                return;
            }
        }
    }
}