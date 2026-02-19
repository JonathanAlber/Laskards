using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Player;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents an effect that allows the player to draw extra cards.
    /// </summary>
    public sealed class ExtraDrawPlayerEffect : PlayerEffect
    {
        public ExtraDrawPlayerEffect(PlayerController target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData, int extraDraws)
            : base(target, creatorTeam, durationType, duration, effectData)
        {
            AdditionalActionCardDraws = extraDraws;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            return new Dictionary<string, object>
            {
                { "ExtraCardsToDraw", AdditionalActionCardDraws },
                { "Duration", RemainingDuration }
            };
        }
    }
}