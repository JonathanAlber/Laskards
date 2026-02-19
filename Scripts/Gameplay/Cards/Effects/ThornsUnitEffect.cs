using System.Collections.Generic;
using Gameplay.Board;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Units;
using Gameplay.Units.Data;
using UnityEngine;
using Utility.Logging;
using Utility.Types;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents a thorns effect that reflects a percentage of damage back to attackers.
    /// </summary>
    public class ThornsUnitEffect : UnitEffect
    {
        public readonly float DamageReflectionPercentage;

        public ThornsUnitEffect(UnitController target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData, float damageReflectionPercentage)
            : base(target, creatorTeam, durationType, duration, effectData)
        {
            DamageReflectionPercentage = damageReflectionPercentage;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            return new Dictionary<string, object>
            {
                { "DamageReflectionPercentage", PercentageUtils.ToPercentString(DamageReflectionPercentage) },
                { "Duration", RemainingDuration }
            };
        }

        protected override void OnApply()
        {
            base.OnApply();
            Target.OnDamaged += ReflectDamage;
        }

        protected override void OnRevert()
        {
            base.OnRevert();
            Target.OnDamaged -= ReflectDamage;
        }

        private void ReflectDamage(UnitController _, UnitController attacker, int damage, EDamageKind damageKind)
        {
            if (damageKind == EDamageKind.Reflection)
                return;

            if (attacker == null)
            {
                CustomLogger.LogWarning("Attacker is null. Skipping thorns reflection.", null);
                return;
            }

            if (damage <= 0)
            {
                CustomLogger.LogWarning("Received non-positive damage value. Skipping thorns reflection.", null);
                return;
            }

            if (!attacker.CanBeAttacked())
            {
                Tile tile = attacker.CurrentTile;
                CustomLogger.LogWarning("Active effect hinder reflecting to the attacker at" +
                                        $"({GridCoordinateFormatter.ToA1(tile.Row, tile.Column)}).", null);
                return;
            }

            int reflectedDamage = Mathf.RoundToInt(damage * DamageReflectionPercentage);
            if (reflectedDamage > 0)
                attacker.ApplyDamage(reflectedDamage, Target, EDamageKind.Reflection);
        }
    }
}