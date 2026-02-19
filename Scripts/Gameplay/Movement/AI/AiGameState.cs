using System.Collections.Generic;
using Gameplay.CardExecution;
using UnityEngine;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Immutable snapshot of the game state used by the AI search.
    /// </summary>
    public sealed class AiGameState
    {
        /// <summary>
        /// Fast board-indexed lookup for units.
        /// </summary>
        public readonly FlattenedArray<AiUnitSnapshot> UnitAt;

        /// <summary>
        /// Total number of rows on the board.
        /// </summary>
        public readonly int Rows;

        /// <summary>
        /// Total number of columns on the board.
        /// </summary>
        public readonly int Columns;

        /// <summary>
        /// Current health of the player hero.
        /// </summary>
        public readonly int PlayerHp;

        /// <summary>
        /// Immutable list of all units in this state.
        /// </summary>
        public readonly IReadOnlyList<AiUnitSnapshot> Units;

        /// <summary>
        /// Flattened array marking which tiles contain any tile effects.
        /// </summary>
        public readonly FlattenedArray<List<AiTileEffectSnapshot>> TileEffects;

        /// <summary>
        /// Creates a new immutable game state snapshot.
        /// </summary>
        public AiGameState(int rows, int columns, int playerHp, IReadOnlyList<AiUnitSnapshot> units,
            FlattenedArray<List<AiTileEffectSnapshot>> tileEffects)
        {
            Rows = rows;
            Columns = columns;
            PlayerHp = playerHp;
            Units = units;
            TileEffects = tileEffects;
            UnitAt = new FlattenedArray<AiUnitSnapshot>(columns, rows);
            InitializeUnitAt(units);
        }

        /// <summary>
        /// Returns true if any tile effect blocks movement.
        /// </summary>
        public bool IsTileBlocked(int row, int column)
        {
            List<AiTileEffectSnapshot> list = TileEffects[column, row];
            if (list == null)
            {
                CustomLogger.LogWarning($"Tile effects list is null at ({row}, {column}).", null);
                return false;
            }

            foreach (AiTileEffectSnapshot tileEffect in list)
                if (tileEffect.BlocksTile)
                    return true;

            return false;
        }

        /// <summary>
        /// Applies a move and returns a new immutable successor state.
        /// </summary>
        public AiGameState ApplyMove(AiMove move, ETeam movingTeam)
        {
            if (move.IsPass)
                return this;

            AiUnitSnapshot movingUnit = GetUnitById(move.UnitId);
            if (movingUnit == null)
            {
                CustomLogger.LogWarning($"Trying to apply move for non-existing unit ID {move.UnitId}.", null);
                return this;
            }

            if (movingUnit.Team != movingTeam)
                return this;

            AiUnitSnapshot targetUnit = UnitAt[move.ToColumn, move.ToRow];
            bool isCapture = targetUnit != null && targetUnit.Team != movingTeam;

            AiGameState captureState = ApplyCapture(isCapture, movingUnit, targetUnit);
            if (captureState != null)
                return captureState;

            int newPlayerHp = PlayerHp;
            List<AiUnitSnapshot> updatedUnits = new(Units.Count);

            bool movingUnitDies = false;
            bool moved = false;
            bool reachesEnemyBackRow = IsMovingIntoEnemyBackRow(move, movingTeam);

            foreach (AiUnitSnapshot unit in Units)
            {
                if (!unit.IsAlive)
                    continue;

                if (isCapture && unit.Id == targetUnit.Id)
                    continue;

                if (unit.Id != movingUnit.Id)
                {
                    updatedUnits.Add(unit);
                    continue;
                }

                HandleBackRowEffects(reachesEnemyBackRow, movingTeam, movingUnit, newPlayerHp,
                    out bool backRowKillsUnit, out int updatedHp);

                if (backRowKillsUnit)
                {
                    newPlayerHp = updatedHp;
                    movingUnitDies = true;
                    continue;
                }

                int lifetime = movingUnit.Lifetime;
                int movesLeft = movingUnit.MovesLeft;

                ApplyLifetimeTick(reachesEnemyBackRow, ref lifetime, ref movingUnitDies);
                if (movingUnitDies)
                    continue;

                if (movesLeft > 0)
                    movesLeft -= 1;

                AiUnitSnapshot movedUnit = CreateMovedUnitSnapshot(movingUnit, move, lifetime, movesLeft);
                updatedUnits.Add(movedUnit);
                moved = true;
            }

            if (!moved && !movingUnitDies && !isCapture)
                return this;

            return new AiGameState(Rows, Columns, newPlayerHp, updatedUnits, TileEffects);
        }

        /// <summary>
        /// Applies boss back-row damage and determines if the moving unit should die.
        /// </summary>
        private static void HandleBackRowEffects(bool reachesBackRow, ETeam movingTeam, AiUnitSnapshot unit,
            int currentPlayerHp, out bool unitDies, out int updatedPlayerHp)
        {
            unitDies = false;
            updatedPlayerHp = currentPlayerHp;

            if (!reachesBackRow)
                return;

            if (movingTeam != ETeam.Boss)
                return;

            int hpAfter = currentPlayerHp - unit.GetEffectiveDamage();
            if (hpAfter < 0)
                hpAfter = 0;

            updatedPlayerHp = hpAfter;
            unitDies = true;
        }

        private static void ApplyLifetimeTick(bool reachesBackRow, ref int lifetime, ref bool movingUnitDies)
        {
            if (reachesBackRow)
                return;

            if (lifetime <= 0)
                return;

            lifetime -= 1;
            if (lifetime <= 0)
                movingUnitDies = true;
        }

        private static AiUnitSnapshot CreateMovedUnitSnapshot(AiUnitSnapshot unit, AiMove move, int lifetime,
            int movesLeft)
        {
            return unit
                .ToBuilder()
                .SetPosition(move.ToRow, move.ToColumn)
                .SetLifetime(lifetime)
                .SetMovesLeft(movesLeft)
                .Build();
        }

        private AiUnitSnapshot GetUnitById(int id)
        {
            foreach (AiUnitSnapshot unit in Units)
                if (unit.Id == id)
                    return unit;

            return null;
        }

        private AiGameState ApplyCapture(bool isCapture, AiUnitSnapshot attacker, AiUnitSnapshot defender)
        {
            if (!isCapture)
                return null;

            List<AiUnitSnapshot> newUnits = new(Units.Count);

            // Check if defender has thorns
            int attackerDamage = attacker.GetEffectiveDamage();
            if (attacker.CanBeAttacked)
            {
                foreach (AiUnitEffectSnapshot effect in defender.Effects)
                {
                    float thornsReflectionPercentage = effect.ThornsReflectionPercentage;
                    if (thornsReflectionPercentage <= 0f)
                        continue;

                    int damageToAttacker = Mathf.RoundToInt(attackerDamage * thornsReflectionPercentage);
                    int attackerRemainingHp = attacker.CurrentHealth - damageToAttacker;
                    if (attackerRemainingHp <= 0)
                    {
                        // Attacker also dies, so remove it from the state
                        foreach (AiUnitSnapshot unit in Units)
                        {
                            if (unit.Id == attacker.Id || unit.Id == defender.Id)
                                continue;

                            newUnits.Add(unit);
                        }

                        return new AiGameState(Rows, Columns, PlayerHp, newUnits, TileEffects);
                    }

                    // Update attacker health
                    attacker = attacker
                        .ToBuilder()
                        .SetHealth(attackerRemainingHp)
                        .Build();
                }
            }

            int remainingHp = defender.CurrentHealth - attackerDamage;
            if (remainingHp <= 0)
                return null;

            AiUnitSnapshot damagedTarget = defender
                .ToBuilder()
                .SetHealth(remainingHp)
                .Build();

            foreach (AiUnitSnapshot unit in Units)
                newUnits.Add(unit.Id == defender.Id ? damagedTarget : unit);

            return new AiGameState(Rows, Columns, PlayerHp, newUnits, TileEffects);
        }

        private bool IsMovingIntoEnemyBackRow(AiMove move, ETeam movingTeam)
        {
            int lastRowIndex = Rows - 1;

            switch (movingTeam)
            {
                case ETeam.Player when move.ToRow == lastRowIndex:
                case ETeam.Boss when move.ToRow == 0:
                    return true;
                default:
                    return false;
            }
        }

        private void InitializeUnitAt(IReadOnlyList<AiUnitSnapshot> units)
        {
            foreach (AiUnitSnapshot u in units)
            {
                if (u == null)
                {
                    CustomLogger.LogWarning("Encountered null unit in constructor.", null);
                    continue;
                }

                UnitAt[u.Column, u.Row] = u;
            }
        }
    }
}