using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.StatLayers.Units;
using Gameplay.Units;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents an effect that provides bonus moves to a unit.
    /// </summary>
    public class BonusMovesUnitEffect : UnitEffectWithLayer
    {
        private readonly int _bonusMoves;

        public BonusMovesUnitEffect(UnitController target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData, IUnitStatLayer layer, int bonusMoves)
            : base(target, creatorTeam, durationType, duration, effectData, layer)
        {
            _bonusMoves = bonusMoves;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            return new Dictionary<string, object>
            {
                { "BonusMoves", _bonusMoves },
                { "Duration", RemainingDuration }
            };
        }
    }
}