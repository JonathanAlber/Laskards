using System.Collections.Generic;
using Gameplay.Board;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects;
using Gameplay.Player;
using Gameplay.StatLayers.Units;
using Gameplay.Units;
using Systems.Services;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Translates the live Unity game state into an immutable AI state.
    /// Also builds a mapping from AI unit ids back to live UnitControllers.
    /// </summary>
    public sealed class BossAiStateBuilder
    {
        private readonly GameBoard _board;
        private readonly UnitManager _unitManager;
        private readonly PlayerController _playerController;

        public BossAiStateBuilder(GameBoard board, UnitManager unitManager, PlayerController playerController)
        {
            _board = board;
            _unitManager = unitManager;
            _playerController = playerController;
        }

        /// <summary>
        /// Attempts to create a state builder using the global service locator.
        /// Returns null if required services are missing.
        /// </summary>
        public static BossAiStateBuilder TryCreateFromServices()
        {
            if (!ServiceLocator.TryGet(out GameBoard gameBoard))
                return null;

            if (!ServiceLocator.TryGet(out UnitManager unitManagerInstance))
                return null;

            if (!ServiceLocator.TryGet(out PlayerController playerControllerInstance))
                return null;

            return new BossAiStateBuilder(gameBoard, unitManagerInstance, playerControllerInstance);
        }

        /// <summary>
        /// Builds an AI state and a mapping from AI ids back to live unit controllers.
        /// </summary>
        public AiGameState Build(out Dictionary<int, UnitController> idToUnit)
        {
            idToUnit = new Dictionary<int, UnitController>();

            List<AiUnitSnapshot> units = new();

            int rows = _board.BoardConfiguration.Rows;
            int columns = _board.BoardConfiguration.Columns;

            FlattenedArray<List<AiTileEffectSnapshot>> tileEffects = new(columns, rows);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Tile tile = _board.GetTile(r, c);
                    if (tile == null)
                    {
                        CustomLogger.LogWarning($"Tile is null at ({GridCoordinateFormatter.ToA1(r, c)})," +
                                                " when building AI state.", null);
                        continue;
                    }

                    List<AiTileEffectSnapshot> effects = new();

                    foreach (TileEffect eff in tile.ActiveEffects)
                    {
                        int remaining = eff.DurationType == EDurationType.Temporary ? eff.RemainingDuration : -1;
                        effects.Add(new AiTileEffectSnapshot(eff.DurationType, remaining, eff.OccupiesTile));
                    }

                    tileEffects[c, r] = effects;
                }
            }

            int nextId = 0;

            AddUnits(_unitManager.AllUnits, ref nextId, units, idToUnit);

            int playerHp = _playerController.Snapshot.CurrentHp;

            AiGameState state = new(rows, columns, playerHp, units, tileEffects);
            return state;
        }

        private static void AddUnits(IReadOnlyList<UnitController> liveUnits, ref int nextId, List<AiUnitSnapshot> snapshots,
            Dictionary<int, UnitController> idToUnit)
        {
            foreach (UnitController unit in liveUnits)
            {
                if (unit == null || unit.Model is not { IsAlive: true } || unit.CurrentTile == null)
                    continue;

                List<AiUnitEffectSnapshot> effects = new();
                foreach (UnitEffect eff in unit.ActiveEffects)
                {
                    int remaining = eff.DurationType == EDurationType.Temporary
                        ? eff.RemainingDuration
                        : -1;

                    IUnitStatLayer statLayer = null;
                    if (eff is UnitEffectWithLayer withLayer)
                        statLayer = withLayer.StatLayer;

                    float thornsReflectionPercentage = 0f;
                    if (eff is ThornsUnitEffect thornsEff)
                        thornsReflectionPercentage = thornsEff.DamageReflectionPercentage;

                    effects.Add(new AiUnitEffectSnapshot(eff.DurationType, remaining, eff.CanBeAttacked, statLayer,
                        thornsReflectionPercentage));
                }

                int movesLeftNow = unit.Model.RemainingMoves;

                AiUnitSnapshot snap = new(nextId, unit.Team, unit.Model.UnitType, unit.CurrentTile.Row,
                    unit.CurrentTile.Column, unit.Model.CurrentHealth, unit.Model.BaseDamage, unit.Model.Lifetime,
                    unit.Model.Worth, movesLeftNow, effects);

                snapshots.Add(snap);
                idToUnit[nextId] = unit;
                nextId++;
            }
        }
    }
}