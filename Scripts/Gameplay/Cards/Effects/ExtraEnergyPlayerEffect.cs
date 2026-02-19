using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Player;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Player effect that grants extra energy per turn.
    /// </summary>
    public sealed class ExtraEnergyPlayerEffect : PlayerEffect
    {
        public ExtraEnergyPlayerEffect(PlayerController target, ETeam creatorTeam, EDurationType durationType,
            int duration, EffectData effectData, int extraEnergy)
            : base(target, creatorTeam, durationType, duration, effectData)
        {
            AdditionalEnergyGain = extraEnergy;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            return new Dictionary<string, object>
            {
                { "ExtraEnergyToGain", AdditionalEnergyGain },
                { "Duration", RemainingDuration }
            };
        }
    }
}