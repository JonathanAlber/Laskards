using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Player;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="ExtraEnergyModifierData"/>.
    /// </summary>
    public class ExtraEnergyModifierRuntimeState : TypedModifierRuntimeState<ExtraEnergyModifierData, PlayerController>
    {
        private readonly int _extraEnergyToGain;

        public ExtraEnergyModifierRuntimeState(ExtraEnergyModifierData data) : base(data)
        {
            _extraEnergyToGain = data.ExtraEnergyToGain;
        }

        protected override IEffect CreateEffect(PlayerController target, ETeam creatorTeam)
        {
            return new ExtraEnergyPlayerEffect(target, creatorTeam, DurationType, Duration, EffectData,
                _extraEnergyToGain);
        }
    }
}