using System.Collections.Generic;
using System.Linq;
using Gameplay.Board;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Gameplay.Movement.AI;
using Gameplay.Units;
using Systems.Services;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.CardExecution.Targeting
{
    /// <summary>
    /// AI resolver for unit card targets.
    /// </summary>
    public class AiUnitCardResolver : AiCardResolver<UnitCardModel>
    {
        public AiUnitCardResolver(int searchDepth, BossMinimaxSearch minimax) : base(searchDepth, minimax) { }

        public override bool TryResolveTarget(UnitCardModel unitCardModel, out IModifiableBase target)
        {
            target = null;

            if (unitCardModel == null)
            {
                CustomLogger.LogWarning("Received null unit card model.", null);
                return false;
            }

            if (!ServiceLocator.TryGet(out GameBoard board))
                return false;

            if (!ServiceLocator.TryGet(out UnitPlacementValidator unitPlacementValidator))
                return false;

            // Collect all free tiles on the boss back row
            int backRow = board.BoardConfiguration.Rows - unitPlacementValidator.Deployment.BossRowsFromTop;
            List<Tile> candidateTiles = Enumerable.Range(0, board.BoardConfiguration.Columns)
                .Select(x => board.GetTile(backRow, x))
                .Where(tile => tile != null && !tile.IsOccupied())
                .ToList();

            switch (candidateTiles.Count)
            {
                case 0:
                {
                    CustomLogger.LogWarning("No valid spawn tiles available for boss unit.", null);
                    return false;
                }
                // If there is only one choice, no need to consult minimax.
                case 1:
                {
                    target = candidateTiles[0];
                    return true;
                }
            }

            if (SearchDepth <= 0)
            {
                CustomLogger.LogWarning("AI not properly initialized, defaulting to random placement.", null);
                int index = Random.Range(0, candidateTiles.Count);
                target = candidateTiles[index];
                return true;
            }

            // Build an AI snapshot from the current live state.
            BossAiStateBuilder builder = BossAiStateBuilder.TryCreateFromServices();
            if (builder == null)
            {
                CustomLogger.LogWarning("Failed to create AI state builder, defaulting to random placement.", null);
                int index = Random.Range(0, candidateTiles.Count);
                target = candidateTiles[index];
                return true;
            }

            AiGameState baseState = builder.Build(out _);

            // Evaluate each candidate spawn tile by simulating the unit being spawned there
            float bestScore = float.NegativeInfinity;
            Tile bestTile = null;

            foreach (Tile tile in candidateTiles)
            {
                AiGameState simulatedState = SimulateSpawn(baseState, unitCardModel, tile);
                float score = Minimax.EvaluateStateWithSearch(simulatedState, SearchDepth, ETeam.Boss);

                if (score <= bestScore)
                    continue;

                bestScore = score;
                bestTile = tile;
            }

            if (bestTile == null)
            {
                CustomLogger.LogWarning("Minimax failed to evaluate spawn tiles, defaulting to random placement.", null);
                int index = Random.Range(0, candidateTiles.Count);
                target = candidateTiles[index];
                return true;
            }

            target = bestTile;
            return true;
        }

        private static AiGameState SimulateSpawn(AiGameState baseState, UnitCardModel unitCardModel, Tile spawnTile)
        {
            if (baseState == null)
            {
                CustomLogger.LogWarning("Tried to simulate spawn on a null AI game state.", null);
                return null;
            }

            if (unitCardModel == null)
            {
                CustomLogger.LogWarning("Tried to simulate spawn with a null unit card model.", null);
                return baseState;
            }

            if (spawnTile == null)
            {
                CustomLogger.LogWarning("Tried to simulate spawn on a null tile.", null);
                return baseState;
            }

            UnitModel tempModel = new(unitCardModel, unitCardModel.UnitData, ETeam.Boss);
            int nextId = 0;
            foreach (AiUnitSnapshot u in baseState.Units)
            {
                if (u.Id >= nextId)
                    nextId = u.Id + 1;
            }

            List<AiUnitSnapshot> units = new(baseState.Units.Count + 1);
            foreach (AiUnitSnapshot u in baseState.Units)
                units.Add(u);

            AiUnitSnapshot spawned = new(
                nextId,
                ETeam.Boss,
                tempModel.UnitType,
                spawnTile.Row,
                spawnTile.Column,
                tempModel.CurrentHealth,
                tempModel.BaseDamage,
                tempModel.Lifetime,
                tempModel.Worth,
                UnitModel.BaseMovesPerTurn,
                new List<AiUnitEffectSnapshot>()
            );

            units.Add(spawned);
            return new AiGameState(baseState.Rows, baseState.Columns, baseState.PlayerHp, units, baseState.TileEffects);
        }
    }
}