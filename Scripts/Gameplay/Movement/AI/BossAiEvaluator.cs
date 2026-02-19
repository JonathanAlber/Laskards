using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Movement.AI.Data;
using Gameplay.Units.Movement;
using Systems.Services;
using UnityEngine;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Static evaluation function for boss AI positions.
    /// Positive values are good for the boss, negative for the player.
    /// </summary>
    public sealed class BossAiEvaluator
    {
        /// <summary>
        /// Score representing a winning state for the boss.
        /// Not maximum float value to avoid alpha-beta pruning issues.
        /// </summary>
        private const float WinningScore = 100000f;

        private readonly BossAiEvaluationSettings _settings;
        private readonly UnitPlacementValidator _placementValidator;
        private readonly AiUnitValueCalculator _unitValueCalculator;

        /// <summary>
        /// Creates a new boss evaluator using the provided settings.
        /// </summary>
        public BossAiEvaluator(BossAiEvaluationSettings settings, AiUnitValueCalculator aiUnitValueCalculator)
        {
            _settings = settings;
            _unitValueCalculator = aiUnitValueCalculator;

            ServiceLocator.TryGet(out _placementValidator);
        }

        /// <summary>
        /// Returns a score for the given state from the boss perspective.
        /// </summary>
        public float Evaluate(AiGameState state, int depth)
        {
            if (state.PlayerHp <= 0)
                return WinningScore + depth;

            Dictionary<int, float> valueCache = new(state.Units.Count);

            BossMoveEvaluation eval = new()
            {
                // Player HP impact
                playerHp = (0 - state.PlayerHp) * _settings.PlayerHealthWeight
            };

            foreach (AiUnitSnapshot unit in state.Units)
            {
                if (!unit.IsAlive)
                {
                    CustomLogger.LogWarning($"Evaluating dead unit {unit.Id}, skipping.", null);
                    continue;
                }

                float value = _unitValueCalculator.Compute(unit);
                valueCache[unit.Id] = value;

                // Unit values
                if (unit.Team == ETeam.Boss)
                {
                    eval.bossUnits += value * _settings.UnitValueWeight;
                    eval.bossUnitRawSum += value;
                    eval.bossUnitCount++;

                    if (_settings.EnableBossUnitValueLogging)
                    {
                        UnitValueBreakdown breakdown = _unitValueCalculator.ComputeWithBreakdown(unit);
                        CustomLogger.Log($"[BossUnitValue] {breakdown}", null);
                    }
                }
                else
                {
                    eval.playerUnits -= value * _settings.UnitValueWeight;
                }

                // Positional scoring (forward progress / territory)
                if (unit.Team == ETeam.Boss)
                {
                    // Players retreating from the boss's back row is good for the boss
                    int distanceFromBossBackRow = state.Rows - 1 - unit.Row;
                    eval.bossPosition += distanceFromBossBackRow * _settings.PositionWeight;
                }
                else
                {
                    // Player approaching the boss's back row is bad for the boss
                    int progressTowardsBoss = unit.Row; // 0 at spawn, increasing as they advance
                    eval.playerPosition -= progressTowardsBoss
                                           * _settings.PositionWeight
                                           * _settings.PlayerPositionPenaltyMultiplier;
                }

                // Center control
                int centerRow = state.Rows / 2;
                int distFromCenter = Mathf.Abs(unit.Row - centerRow);
                float centerScore = (centerRow > 0
                        ? 1f - distFromCenter / (float)centerRow
                        : 0f)
                        * _settings.CenterPositionWeight;

                if (unit.Team == ETeam.Boss)
                    eval.centerBias += centerScore;
                else
                    eval.centerBias -= centerScore;

                switch (unit.Team)
                {
                    //Lifetime adjustments (Boss only)
                    case ETeam.Boss:
                    {
                        switch (unit.Lifetime)
                        {
                            case -1:
                                eval.lifetime += value * _settings.LifetimeInfiniteBonusWeight;
                                break;
                            case 1:
                                eval.lifetime -= value * _settings.LifetimeRiskWeight;
                                break;
                            case > 1 and < 4:
                                eval.lifetime -= value * _settings.LifetimeSoftPenalty;
                                break;
                        }
                        break;
                    }
                    case ETeam.Player:
                    {
                        // Player units close to the boss's back row are dangerous.
                        int distFromBossBackRow = state.Rows - 1 - unit.Row;
                        if (distFromBossBackRow <= 1)
                        {
                            float threatFactor = 1f - distFromBossBackRow;
                            eval.playerThreats -= unit.GetEffectiveDamage() * threatFactor * _settings.PlayerBackRowThreatWeight;
                        }
                        break;
                    }
                }
            }

            // Spawn-threat (pawn from back row) handling
            float spawnThreatPenalty = EvaluateSpawnThreats(state, valueCache);
            eval.spawnThreats += spawnThreatPenalty;

            AiAttackMapBuilder.BuildAttackMaps(state, out FlattenedArray<int> playerAttackMap,
                out FlattenedArray<int> bossAttackMap);

            // Tactical danger (attacked + defended / undefended boss units)
            float dangerPenalty = EvaluateDanger(state, valueCache, playerAttackMap, bossAttackMap);
            eval.dangerPenalty += dangerPenalty;

            // Mobility (how many legal moves each side has)
            EvaluateMobility(state, out float bossMobility, out float playerMobility);
            float mobilityScore =
                (bossMobility - playerMobility * _settings.PlayerMobilityPenaltyMultiplier) *
                _settings.MobilityWeight;
            eval.mobility += mobilityScore;

            float total = eval.Total;

            if (_settings.EnableDetailLogging)
                LogBreakdown(eval, total);

            return total;
        }

        private static void LogBreakdown(BossMoveEvaluation bd, float total)
        {
            CustomLogger.Log(
                $" Total = {total:F3}\n" +
                $"  PlayerHP = {bd.playerHp:F3}\n" +
                $"  BossUnitValue = {bd.bossUnits:F3}\n" +
                $"(count={bd.bossUnitCount}, rawSum={bd.bossUnitRawSum:F3})\n" +
                $"  PlayerUnitValue = {bd.playerUnits:F3}\n" +
                $"  BossPosition = {bd.bossPosition:F3}\n" +
                $"  PlayerPosition = {bd.playerPosition:F3}\n" +
                $"  CenterBias = {bd.centerBias:F3}\n" +
                $"  Lifetime = {bd.lifetime:F3}\n" +
                $"  BackRowThreats = {bd.playerThreats:F3}\n" +
                $"  SpawnThreats = {bd.spawnThreats:F3}\n" +
                $"  DangerPenalty = {bd.dangerPenalty:F3}\n" +
                $"  Mobility = {bd.mobility:F3}",
                null);
        }

        private static void EvaluateMobility(AiGameState state, out float bossMobility, out float playerMobility)
        {
            bossMobility = 0f;
            playerMobility = 0f;

            int rows = state.Rows;
            int cols = state.Columns;

            foreach (AiUnitSnapshot unit in state.Units)
            {
                if (!unit.IsAlive)
                    continue;

                UnitMovementDefinition def = StandardUnitMovementLibrary.GetDefinition(unit.UnitType);
                if (def == null)
                    continue;

                int forwardSign = unit.Team == ETeam.Player ? 1 : -1;
                int mobility = 0;

                foreach (UnitMovementRule rule in def.Rules)
                {
                    int dirX = rule.Direction.X;
                    int dirY = rule.Direction.Y * forwardSign;

                    if (rule.IsJump)
                    {
                        int tr = unit.Row + dirY;
                        int tc = unit.Column + dirX;

                        if (!AiAttackMapBuilder.Inside(state, tr, tc))
                            continue;

                        if (state.IsTileBlocked(tr, tc))
                            continue;

                        AiUnitSnapshot occ = state.UnitAt[tc, tr];

                        if (occ == null)
                        {
                            if (rule.CanMoveToEmpty)
                                mobility++;
                        }
                        else
                        {
                            if (occ.Team == unit.Team)
                                continue;

                            if (!occ.CanBeAttacked)
                                continue;

                            if (rule.CanCapture)
                                mobility++;
                        }
                    }
                    else
                    {
                        // Sliding
                        for (int s = 1; s <= rule.MaxSteps; s++)
                        {
                            int tr = unit.Row + dirY * s;
                            int tc = unit.Column + dirX * s;

                            if (tr < 0 || tr >= rows || tc < 0 || tc >= cols)
                                break;

                            if (state.IsTileBlocked(tr, tc))
                                break;

                            AiUnitSnapshot occ = state.UnitAt[tc, tr];

                            if (occ == null)
                            {
                                if (rule.CanMoveToEmpty)
                                    mobility++;
                                continue;
                            }

                            if (occ.Team == unit.Team)
                                break;

                            if (!occ.CanBeAttacked)
                                break;

                            if (rule.CanCapture)
                                mobility++;

                            break;
                        }
                    }
                }

                if (unit.Team == ETeam.Boss)
                    bossMobility += mobility;
                else
                    playerMobility += mobility;
            }
        }

        private float EvaluateSpawnThreats(AiGameState state, Dictionary<int, float> effectiveValues)
        {
            float penalty = 0f;
            const int playerBackRow = 0;

            foreach (AiUnitSnapshot unit in state.Units)
            {
                if (unit is not { IsAlive: true } || unit.Team != ETeam.Boss)
                    continue;

                // Determine if this unit is within the spawn-danger zone
                int distFromPlayerBackRow = unit.Row - playerBackRow;
                if (distFromPlayerBackRow <= 0 ||
                    distFromPlayerBackRow > _placementValidator.Deployment.PlayerRowsFromBottom)
                    continue;

                // Only spaces directly above spawn row are directly capturable by a diagonally-moving pawn
                if (unit.Row != playerBackRow + 1)
                    continue;

                // Check diagonal spawn tiles
                int unitColumn = unit.Column;
                int[] spawnColumns = { unitColumn - 1, unitColumn + 1 };
                int threatsCount = 0;

                foreach (int spawnColumn in spawnColumns)
                {
                    if (spawnColumn < 0 || spawnColumn >= state.Columns)
                        continue;

                    if (state.IsTileBlocked(playerBackRow, spawnColumn))
                        continue;

                    AiUnitSnapshot occupant = state.UnitAt[spawnColumn, playerBackRow];
                    if (occupant != null)
                        continue;

                    threatsCount++;
                }

                if (threatsCount == 0)
                    continue;

                // base spawn penalty (normal danger)
                float unitValue = effectiveValues[unit.Id];
                float basePenalty = unitValue * threatsCount * _settings.SpawnThreatWeight;

                // If the unit has any effect preventing attack, scale penalty down
                float reductionMultiplier = 1f;
                foreach (AiUnitEffectSnapshot eff in unit.Effects)
                {
                    if (eff.CanBeAttacked)
                        continue;

                    // Apply main unattackable reduction
                    reductionMultiplier *= _settings.SpawnThreatUnattackableMultiplier;

                    // Apply duration-based scaling (longer duration equals more reduction)
                    if (eff.DurationType == EDurationType.Temporary &&
                        eff.RemainingDuration > 0)
                    {
                        reductionMultiplier *= 1f - eff.RemainingDuration
                            * _settings.SpawnThreatInvulnerabilityDurationMultiplier;
                    }
                }

                reductionMultiplier = Mathf.Clamp(reductionMultiplier, _settings.SpawnThreatMinMultiplier, 1f);
                penalty -= basePenalty * reductionMultiplier;
            }

            return penalty;
        }

        private float EvaluateDanger(AiGameState state, Dictionary<int, float> effectiveValues,
            FlattenedArray<int> playerAttackMap, FlattenedArray<int> bossAttackMap)
        {
            float penalty = 0f;

            foreach (AiUnitSnapshot bossUnit in state.Units)
            {
                if (bossUnit.Team != ETeam.Boss || !bossUnit.IsAlive)
                    continue;

                int attackers = playerAttackMap[bossUnit.Column, bossUnit.Row];
                if (attackers <= 0)
                    continue;

                int defenders = bossAttackMap[bossUnit.Column, bossUnit.Row];

                // More attackers = worse
                float multi = 1f + _settings.DangerAmountMultiplier * (attackers - 1);

                // If not defended at all, extra penalty
                if (defenders == 0)
                    multi += _settings.DangerUndefendedMultiplier;

                float effectiveValue = effectiveValues[bossUnit.Id];
                penalty -= effectiveValue * multi * _settings.DangerWeight;
            }

            return penalty;
        }
    }
}