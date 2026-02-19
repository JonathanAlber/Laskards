using Gameplay.Cards.Data;
using Gameplay.Cards.Effects;
using Gameplay.StatLayers.Units;
using Gameplay.Units.Worth.Data;
using Systems.Services;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Units.Worth
{
    /// <summary>
    /// Calculates the worth of a unit based on its stats and predefined weights.
    /// </summary>
    public class UnitWorthCalculator : GameServiceBehaviour
    {
        [field: SerializeField] public UnitWorthWeights WorthWeights { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (WorthWeights != null)
                return;

            CustomLogger.LogError($"The {nameof(UnitWorthWeights)} ScriptableObject is not assigned.", this);
        }

        /// <summary>
        /// Calculates the worth of the given unit based on its stats and predefined weights.
        /// </summary>
        /// <param name="unit">The unit to evaluate.</param>
        /// <returns>The calculated worth of the unit.</returns>
        public float CalculateWorth(UnitController unit)
        {
            UnitModel model = unit.Model;
            UnitWorthWeights w = WorthWeights;

            float value = 0f;
            int baseDamage = model.BaseDamage;

            // Base stats
            value += model.Worth * w.WorthWeight;
            value += model.CurrentHealth * w.CurrentHealthWeight;
            value += baseDamage * w.DamageWeight;
            value += model.RemainingMoves * w.MovesLeftWeight;

            // Lifetime
            if (model.Lifetime == -1)
                value += w.InfiniteLifetimeBonus;
            else
                value += model.Lifetime * w.LifetimeWeight;

            // Invulnerability
            if (!unit.CanBeAttacked())
                value += w.CantBeAttackedBonus;

            // Temporary effects
            float tempDamageValue = 0f;
            float tempMoveValue = 0f;

            foreach (UnitEffect eff in unit.ActiveEffects)
            {
                if (eff is not UnitEffectWithLayer layerEff)
                    continue;

                IUnitStatLayer layer = layerEff.StatLayer;
                if (layer == null)
                    continue;

                int dmgDelta = layer.ModifyDamage(baseDamage) - baseDamage;
                int movDelta = layer.ModifyMoveCount(UnitModel.BaseMovesPerTurn) - UnitModel.BaseMovesPerTurn;

                float mult = 1f;

                // Adjust multiplier for temporary effects
                if (eff.DurationType == EDurationType.Temporary)
                {
                    int t = Mathf.Max(0, eff.RemainingDuration);
                    mult = w.TemporaryEffectBaseMultiplier + t * w.TemporaryEffectPerTurnBonus;
                    mult = Mathf.Clamp(mult, w.TemporaryEffectMinMultiplier, 1f);
                }

                tempDamageValue += dmgDelta * w.DamageWeight * mult;
                tempMoveValue   += movDelta * w.MovesLeftWeight * mult;
            }

            value += tempDamageValue + tempMoveValue;

            return value;
        }
    }
}