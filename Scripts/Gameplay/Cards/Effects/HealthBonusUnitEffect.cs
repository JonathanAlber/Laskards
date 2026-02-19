using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Units;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents an effect that provides a health bonus to a unit.
    /// </summary>
    public class HealthBonusUnitEffect : UnitEffect
    {
        private readonly int _gainedHealth;

        public HealthBonusUnitEffect(UnitController target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData, int gainedHealth)
            : base(target, creatorTeam, durationType, duration, effectData)
        {
            _gainedHealth = gainedHealth;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            return new Dictionary<string, object>
            {
                { "GainedHealth", _gainedHealth },
                { "Duration", RemainingDuration }
            };
        }
    }
}