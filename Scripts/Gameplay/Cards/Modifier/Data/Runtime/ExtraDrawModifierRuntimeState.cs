using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Player;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="ExtraDrawModifierData"/>.
    /// </summary>
    public sealed class ExtraDrawModifierRuntimeState : TypedModifierRuntimeState<ExtraDrawModifierData, PlayerController>
    {
        private readonly int _extraCardsToDraw;

        public ExtraDrawModifierRuntimeState(ExtraDrawModifierData data) : base(data)
        {
            _extraCardsToDraw = data.ExtraCardsToDraw;
        }

        protected override IEffect CreateEffect(PlayerController target, ETeam creatorTeam)
        {
            return new ExtraDrawPlayerEffect(target, creatorTeam, DurationType, Duration, EffectData,
                _extraCardsToDraw);
        }
    }
}