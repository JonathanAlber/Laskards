using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.StatLayers.Units;
using Gameplay.Units;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents an effect that applies an attack multiplier to a unit.
    /// </summary>
    public class AttackMultiplierUnitEffect : UnitEffectWithLayer
    {
        private readonly float _attackMultiplier;

        public AttackMultiplierUnitEffect(UnitController target, ETeam creatorTeam, EDurationType durationType,
            int duration, EffectData effectData, IUnitStatLayer layer, float attackMultiplier)
            : base(target, creatorTeam, durationType, duration, effectData, layer)
        {
            _attackMultiplier = attackMultiplier;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            return new Dictionary<string, object>
            {
                { "AttackMultiplier", _attackMultiplier },
                { "Duration", RemainingDuration }
            };
        }
    }
}