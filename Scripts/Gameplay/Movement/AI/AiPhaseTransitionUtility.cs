using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Provides utilities for progressing immutable AI snapshot states between phases.
    /// This includes ticking temporary effects, removing expired ones and
    /// assigning fresh per-phase move budgets to the active team.
    /// </summary>
    public static class AiPhaseTransitionUtility
    {
        /// <summary>
        /// Creates a new immutable state representing the start of a movement phase
        /// for the specified team. This mirrors the real game.
        /// </summary>
        /// <param name="state">Current snapshot state.</param>
        /// <param name="phaseTeam">Team whose movement phase begins.</param>
        /// <returns>A new immutable AI state reflecting phase transition.</returns>
        public static AiGameState BeginPhase(AiGameState state, ETeam phaseTeam)
        {
            List<AiUnitSnapshot> updatedUnits = new(state.Units.Count);
            bool isBossPhase = phaseTeam == ETeam.Boss;

            foreach (AiUnitSnapshot unit in state.Units)
            {
                if (!unit.IsAlive)
                {
                    AiUnitSnapshot deadSnapshot = unit
                        .ToBuilder()
                        .SetMovesLeft(0)
                        .Build();

                    updatedUnits.Add(deadSnapshot);
                    CustomLogger.LogWarning("Dead unit found in AI state during phase transition. Id: " + unit.Id, null);
                    continue;
                }

                bool isPhaseTeamUnit = unit.Team == phaseTeam;

                // Tick effects only for the team whose PrePlay is happening.
                List<AiUnitEffectSnapshot> updatedEffects = new();
                if (isPhaseTeamUnit)
                {
                    foreach (AiUnitEffectSnapshot effect in unit.Effects)
                    {
                        AiUnitEffectSnapshot ticked = effect.DurationType == EDurationType.Temporary
                            ? effect.Tick()
                            : effect;

                        if (!ticked.IsExpired)
                            updatedEffects.Add(ticked);
                    }
                }
                else
                {
                    foreach (AiUnitEffectSnapshot effect in unit.Effects)
                        updatedEffects.Add(effect);
                }

                int movesLeft = isPhaseTeamUnit ? unit.GetEffectiveMoveCount() : 0;

                AiUnitSnapshot newSnapshot = unit
                    .ToBuilder()
                    .SetEffects(updatedEffects)
                    .SetMovesLeft(movesLeft)
                    .Build();

                updatedUnits.Add(newSnapshot);
            }

            // Tile effects
            FlattenedArray<List<AiTileEffectSnapshot>> newTileEffects = new(state.Columns, state.Rows);
            for (int row = 0; row < state.Rows; row++)
            {
                for (int col = 0; col < state.Columns; col++)
                {
                    List<AiTileEffectSnapshot> oldList = state.TileEffects[col, row];

                    if (!isBossPhase)
                    {
                        List<AiTileEffectSnapshot> copiedList = new();
                        if (oldList != null)
                        {
                            foreach (AiTileEffectSnapshot eff in oldList)
                                copiedList.Add(eff);
                        }

                        newTileEffects[col, row] = copiedList;
                        continue;
                    }

                    // Tick tile effects in the Boss phase
                    List<AiTileEffectSnapshot> updatedList = new();

                    if (oldList != null)
                    {
                        foreach (AiTileEffectSnapshot tileEffect in oldList)
                        {
                            AiTileEffectSnapshot ticked = tileEffect.DurationType == EDurationType.Temporary
                                ? tileEffect.Tick()
                                : tileEffect;

                            if (!ticked.IsExpired)
                                updatedList.Add(ticked);
                        }
                    }

                    newTileEffects[col, row] = updatedList;
                }
            }

            return new AiGameState(state.Rows, state.Columns, state.PlayerHp, updatedUnits, newTileEffects);
        }
    }
}