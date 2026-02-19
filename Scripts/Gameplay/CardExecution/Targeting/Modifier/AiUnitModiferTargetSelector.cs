using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Gameplay.Movement.AI;
using Gameplay.StatLayers.Units;
using Gameplay.Units;
using Systems.Services;
using Utility.Logging;
using Random = UnityEngine.Random;

namespace Gameplay.CardExecution.Targeting.Modifier
{
    /// <summary>
    /// AI resolver for selecting unit targets for action card modifiers.
    /// </summary>
    public class AiUnitModiferTargetSelector : AiActionCardTargetSelector
    {
        public AiUnitModiferTargetSelector(int searchDepth, BossMinimaxSearch minimax) : base(searchDepth, minimax) { }

        public override bool TrySelectTarget(ActionCardModel actionCardModel, out IModifiableBase target)
        {
            target = null;

            if (!ServiceLocator.TryGet(out UnitManager unitManager))
                return false;

            List<UnitController> bossUnits = unitManager.BossUnits.ToList();
            if (bossUnits.Count == 0)
            {
                CustomLogger.Log("No boss units available to target.", null);
                return false;
            }

            if (!TryCreateUnitEffect(bossUnits[0], actionCardModel, out UnitEffect previewUnitEffect))
                return false;

            if (!TryGetValidUnits(bossUnits, previewUnitEffect, out List<UnitController> validUnits))
                return false;

            AiUnitEffectSnapshot snapshot = CreateUnitEffectSnapshot(previewUnitEffect);

            BossAiStateBuilder builder = BossAiStateBuilder.TryCreateFromServices();
            if (builder == null)
            {
                CustomLogger.LogWarning("Failed to create AI state builder, defaulting to random placement.", null);
                int index = Random.Range(0, validUnits.Count);
                target = validUnits[index];
                return true;
            }

            AiGameState baseState = builder.Build(out _);
            return EvaluateBestTarget(baseState, validUnits, unitManager.AllUnits.ToList(), snapshot, out target);
        }

        private bool EvaluateBestTarget(AiGameState baseState, in List<UnitController> validUnits,
            in List<UnitController> allUnits, AiUnitEffectSnapshot snapshot, out IModifiableBase target)
        {
            target = null;

            if (baseState == null)
            {
                CustomLogger.LogWarning("Received null AI game state for target evaluation.", null);
                return false;
            }

            if (allUnits == null || allUnits.Count == 0)
            {
                CustomLogger.LogWarning("No live unit controllers available for target evaluation.", null);
                return false;
            }

            if (SearchDepth <= 0)
            {
                CustomLogger.LogWarning("AI not properly initialized, defaulting to random placement.", null);
                int index = Random.Range(0, validUnits.Count);
                target = validUnits[index];
                return true;
            }

            List<int> validUnitIndexes = new();
            foreach (UnitController unit in validUnits)
            {
                if (unit == null)
                {
                    CustomLogger.LogWarning("Null unit encountered in valid units list during evaluation.", null);
                    continue;
                }

                if (!allUnits.Contains(unit))
                {
                    CustomLogger.LogWarning("Valid unit not found in all units list during evaluation.", null);
                    continue;
                }

                int unitIndex = allUnits.IndexOf(unit);
                validUnitIndexes.Add(unitIndex);
            }

            if (validUnitIndexes.Count == 0)
            {
                CustomLogger.LogWarning("No valid units could be mapped to live unit controllers during evaluation.", null);
                return false;
            }

            float currentScore = Minimax.EvaluateStateWithSearch(baseState, SearchDepth, ETeam.Boss);
            float bestScore = float.NegativeInfinity;
            AiUnitSnapshot bestUnit = null;

            foreach (AiUnitSnapshot unit in baseState.Units)
            {
                if (!validUnitIndexes.Contains(unit.Id))
                    continue;

                AiGameState simulatedState = SimulateEffectApplication(baseState, snapshot, unit);
                float score = Minimax.EvaluateStateWithSearch(simulatedState, SearchDepth, ETeam.Boss);

                if (score <= bestScore)
                    continue;

                bestScore = score;
                bestUnit = unit;
            }

            if (bestUnit == null)
            {
                CustomLogger.LogWarning("Minimax failed to evaluate valid units, defaulting to random placement.", null);
                int index = Random.Range(0, validUnits.Count);
                target = validUnits[index];
                return true;
            }

            if (bestScore < currentScore)
            {
                CustomLogger.Log("No valid unit target improved the AI state. Not executing effect.", null);
                return false;
            }

            if (allUnits.Count <= bestUnit.Id)
            {
                CustomLogger.LogWarning("AI unit ID exceeds live unit controller list bounds.", null);
                return false;
            }

            UnitController bestUnitController = allUnits[bestUnit.Id];
            if (bestUnitController == null)
            {
                CustomLogger.LogWarning("Could not map best AI unit back to live unit controller.", null);
                return false;
            }

            target = bestUnitController;
            return true;
        }

        private static AiGameState SimulateEffectApplication(AiGameState baseState, AiUnitEffectSnapshot snapshot,
            AiUnitSnapshot targetUnit)
        {
            if (baseState == null)
            {
                CustomLogger.LogWarning("Tried to simulate effect application on a null AI game state.", null);
                return null;
            }

            if (targetUnit == null)
            {
                CustomLogger.LogWarning("Tried to simulate effect application on a null target unit.", null);
                return null;
            }

            IReadOnlyList<AiUnitEffectSnapshot> updatedEffects = targetUnit.Effects.Append(snapshot).ToList();
            AiUnitSnapshot modifiedUnit = targetUnit
                .ToBuilder()
                .SetEffects(updatedEffects)
                .Build();

            List<AiUnitSnapshot> modifiedUnits = new();
            foreach (AiUnitSnapshot unit in baseState.Units)
                modifiedUnits.Add(unit.Id == targetUnit.Id ? modifiedUnit : unit);

            return new AiGameState(
                baseState.Rows,
                baseState.Columns,
                baseState.PlayerHp,
                modifiedUnits,
                baseState.TileEffects
            );
        }

        private static AiUnitEffectSnapshot CreateUnitEffectSnapshot(UnitEffect previewUnitEffect)
        {
            IUnitStatLayer statLayer = null;
            if (previewUnitEffect is UnitEffectWithLayer withLayer)
                statLayer = withLayer.StatLayer;

            float thornsReflectionPercentage = 0f;
            if (previewUnitEffect is ThornsUnitEffect thornsEffect)
                thornsReflectionPercentage = thornsEffect.DamageReflectionPercentage;

            AiUnitEffectSnapshot snapshot = new(
                previewUnitEffect.DurationType,
                previewUnitEffect.RemainingDuration,
                previewUnitEffect.CanBeAttacked,
                statLayer,
                thornsReflectionPercentage
            );

            return snapshot;
        }

        private static bool TryCreateUnitEffect(UnitController unit, ActionCardModel actionCardModel,
            out UnitEffect unitEffect)
        {
            unitEffect = null;
            if (!actionCardModel.ModifierState.TryCreateEffect(unit, ETeam.Boss, out IEffect previewEffect))
            {
                CustomLogger.LogWarning("Could not preview modifier effect type.", null);
                return false;
            }

            if (previewEffect is not UnitEffect previewUnitEffect)
            {
                CustomLogger.LogWarning($"Executing effect is not a {nameof(UnitEffect)}.", null);
                return false;
            }

            unitEffect = previewUnitEffect;
            return true;
        }

        private static bool TryGetValidUnits(in List<UnitController> bossUnits , UnitEffect previewEffect,
            out List<UnitController> validUnits)
        {
            validUnits = new List<UnitController>();

            Type effectType = previewEffect.GetType();

            foreach (UnitController unit in bossUnits)
            {
                if (!IsValidBoss(unit))
                    continue;

                if (UnitHasEffect(unit, effectType))
                    continue;

                if (unit.HasMaxEffectsReached())
                    continue;

                validUnits.Add(unit);
            }

            if (validUnits.Count == 0)
            {
                CustomLogger.Log("All boss units already have the effect or cannot receive more effects.", null);
                return false;
            }

            return true;
        }

        private static bool IsValidBoss(UnitController unit)
        {
            if (unit == null)
            {
                CustomLogger.LogWarning("Null unit encountered during boss validation.", null);
                return false;
            }

            if (unit.Model == null)
            {
                CustomLogger.LogWarning($"Unit '{unit.name}' has no model assigned.", unit);
                return false;
            }

            if (unit.Team != ETeam.Boss)
            {
                CustomLogger.LogWarning($"Unit '{unit.name}' is not on the Boss team.", unit);
                return false;
            }

            if (!unit.Model.IsAlive)
            {
                CustomLogger.LogWarning($"Unit '{unit.name}' is not alive.", unit);
                return false;
            }

            return true;
        }

        private static bool UnitHasEffect(UnitController unit, Type effectType)
        {
            return unit.ActiveEffects.Any(effect => effect.GetType() == effectType);
        }
    }
}