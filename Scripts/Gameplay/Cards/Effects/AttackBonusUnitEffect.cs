using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.StatLayers.Units;
using Gameplay.Units;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents an effect that provides an attack bonus to a unit.
    /// </summary>
    public class AttackBonusUnitEffect : UnitEffectWithLayer
    {
        private readonly float _attackIncrease;

        public AttackBonusUnitEffect(UnitController target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData, IUnitStatLayer layer, float attackBonus)
            : base(target, creatorTeam, durationType, duration, effectData, layer)
        {
            _attackIncrease = attackBonus;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            return new Dictionary<string, object>
            {
                { "AttackIncrease", _attackIncrease },
                { "Duration", RemainingDuration }
            };
        }
    }
}