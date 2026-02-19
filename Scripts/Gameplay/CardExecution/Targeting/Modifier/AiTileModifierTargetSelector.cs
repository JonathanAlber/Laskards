using System.Collections.Generic;
using Gameplay.Board;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Gameplay.Movement.AI;
using Systems.Services;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.CardExecution.Targeting.Modifier
{
    /// <summary>
    /// AI resolver for selecting tile targets for action card modifiers.
    /// </summary>
    public class AiTileModifierTargetSelector: AiActionCardTargetSelector
    {
        public AiTileModifierTargetSelector(int searchDepth, BossMinimaxSearch minimax) : base(searchDepth, minimax) { }

        public override bool TrySelectTarget(ActionCardModel actionCardModel, out IModifiableBase target)
        {
            target = null;

            if (!ServiceLocator.TryGet(out GameBoard board))
                return false;

            if (!TryCreateTileEffect(board.Tiles[0,0], actionCardModel, out TileEffect tileEffect))
                return false;

            BossAiStateBuilder builder = BossAiStateBuilder.TryCreateFromServices();
            if (builder == null)
            {
                if (!TrySelectValidTiles(board, out List<Tile> validTiles))
                    return false;

                CustomLogger.LogWarning("Failed to create AI state builder, defaulting to random placement.", null);
                int index = Random.Range(0, validTiles.Count);
                target = validTiles[index];
                return true;
            }

            AiGameState baseState = builder.Build(out _);
            AiTileEffectSnapshot snapshot = CreateTargetTileSnapshot(tileEffect);

            return EvaluateBestTarget(board, baseState, snapshot, out target);
        }

        private bool EvaluateBestTarget(GameBoard board, AiGameState baseState, AiTileEffectSnapshot snapshot,
            out IModifiableBase target)
        {
            target = null;

            float currentScore = Minimax.EvaluateStateWithSearch(baseState, SearchDepth, ETeam.Boss);
            float bestScore = float.NegativeInfinity;
            int bestRow = -1;
            int bestColumn = -1;
            foreach (Tile tile in board.Tiles)
            {
                if (baseState.IsTileBlocked(tile.Row, tile.Column))
                    continue;

                AiGameState simulatedState = SimulateEffectApplication(baseState, tile.Row, tile.Column, snapshot);
                float score = Minimax.EvaluateStateWithSearch(simulatedState, SearchDepth, ETeam.Boss);

                if (score <= bestScore)
                    continue;

                bestScore = score;
                bestRow = tile.Row;
                bestColumn = tile.Column;
            }

            if (bestRow == -1 || bestColumn == -1)
            {
                CustomLogger.LogWarning("Could not find a valid tile to place the modifier effect.", null);
                return false;
            }

            if (bestScore < currentScore)
            {
                CustomLogger.LogWarning("No beneficial tile found for modifier effect placement. Not applying effect.", null);
                return false;
            }

            target = board.Tiles[bestColumn, bestRow];
            return true;
        }

        private static AiGameState SimulateEffectApplication(AiGameState baseState, int tileRow, int tileColumn,
            AiTileEffectSnapshot snapshot)
        {
            List<AiTileEffectSnapshot> effects = new() { snapshot };

            AiGameState copy = new(
                baseState.Rows,
                baseState.Columns,
                baseState.PlayerHp,
                baseState.Units,
                baseState.TileEffects
            );

            copy.TileEffects.Set(tileColumn, tileRow, effects);

            return copy;
        }

        private static AiTileEffectSnapshot CreateTargetTileSnapshot(TileEffect tileEffect)
        {
            return new AiTileEffectSnapshot(
                tileEffect.DurationType,
                tileEffect.RemainingDuration,
                tileEffect.OccupiesTile
            );
        }

        private static bool TryCreateTileEffect(Tile tile, ActionCardModel actionCardModel, out TileEffect tileEffect)
        {
            tileEffect = null;

            if (!actionCardModel.ModifierState.TryCreateEffect(tile, ETeam.Boss, out IEffect previewEffect))
            {
                CustomLogger.LogWarning("Could not preview modifier effect type.", null);
                return false;
            }

            if (previewEffect is not TileEffect previewTileEffect)
            {
                CustomLogger.LogWarning($"Executing effect is not a {nameof(TileEffect)}.", null);
                return false;
            }

            tileEffect = previewTileEffect;
            return true;
        }

        private static bool TrySelectValidTiles(GameBoard board, out List<Tile> validTiles)
        {
            validTiles = new List<Tile>();

            foreach (Tile tile in board.Tiles)
            {
                if (tile.IsOccupied())
                    continue;

                validTiles.Add(tile);
            }

            return validTiles.Count > 0;
        }
    }
}