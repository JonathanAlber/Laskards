using Gameplay.Cards.Data;
using Gameplay.Movement.AI.Data;
using Gameplay.Units;
using Gameplay.Units.Worth.Data;
using UnityEngine;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Computes dynamic unit worth directly from <see cref="AiUnitSnapshot"/> data.
    /// </summary>
    public class AiUnitValueCalculator
    {
        private readonly UnitWorthWeights _weights;

        public AiUnitValueCalculator(UnitWorthWeights unitWorthWeights) => _weights = unitWorthWeights;

        /// <summary>
        /// Computes the dynamic worth of a unit based on its snapshot data.
        /// </summary>
        public float Compute(AiUnitSnapshot snap) => ComputeWithBreakdown(snap).total;

        /// <summary>
        /// Computes the dynamic worth of a unit based on its snapshot data, returning a detailed breakdown.
        /// </summary>
        /// <param name="snap"></param>
        /// <returns></returns>
        public UnitValueBreakdown ComputeWithBreakdown(AiUnitSnapshot snap)
        {
            UnitValueBreakdown bd = new()
            {
                unitId = snap.Id
            };

            float total = 0f;

            // Base stats
            int baseDamage = snap.Damage;

            bd.baseDamagePart = baseDamage * _weights.DamageWeight;
            total += bd.baseDamagePart;

            bd.baseHealthPart = snap.CurrentHealth * _weights.CurrentHealthWeight;
            total += bd.baseHealthPart;

            bd.baseWorthPart = snap.Worth * _weights.WorthWeight;
            total += bd.baseWorthPart;

            bd.baseMovesPart = snap.MovesLeft * _weights.MovesLeftWeight;
            total += bd.baseMovesPart;

            // Lifetime
            if (snap.Lifetime == -1)
            {
                bd.infiniteLifetimeBonus = _weights.InfiniteLifetimeBonus;
                total += bd.infiniteLifetimeBonus;
            }
            else
            {
                bd.lifetimePart = snap.Lifetime * _weights.LifetimeWeight;
                total += bd.lifetimePart;
            }

            // Attackability
            if (!snap.CanBeAttacked)
            {
                bd.unattackableBonus = _weights.CantBeAttackedBonus;
                total += bd.unattackableBonus;
            }

            // Temporary effects
            foreach (AiUnitEffectSnapshot eff in snap.Effects)
            {
                if (eff.StatLayer == null)
                    continue;

                int dmgDelta = eff.StatLayer.ModifyDamage(baseDamage) - baseDamage;
                int movDelta = eff.StatLayer.ModifyMoveCount(UnitModel.BaseMovesPerTurn) - UnitModel.BaseMovesPerTurn;

                float mult = 1f;

                if (eff.DurationType == EDurationType.Temporary)
                {
                    int t = Mathf.Max(0, eff.RemainingDuration);
                    mult = _weights.TemporaryEffectBaseMultiplier + t * _weights.TemporaryEffectPerTurnBonus;
                    mult = Mathf.Clamp(mult, _weights.TemporaryEffectMinMultiplier, 1f);
                }

                bd.tempDamagePart += dmgDelta * _weights.DamageWeight * mult;
                bd.tempMovePart += movDelta * _weights.MovesLeftWeight * mult;
            }

            total += bd.tempDamagePart + bd.tempMovePart;

            // Final
            bd.total = total;
            return bd;
        }
    }
}