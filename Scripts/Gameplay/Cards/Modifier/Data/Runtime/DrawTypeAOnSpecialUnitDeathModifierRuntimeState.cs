using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Player;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for <see cref="DrawTypeAOnSpecialUnitDeathModifierData"/>.
    /// </summary>
    public class DrawTypeAOnSpecialUnitDeathModifierRuntimeState
        : TypedModifierRuntimeState<DrawTypeAOnSpecialUnitDeathModifierData, PlayerController>
    {
        private readonly int _amountToDraw;

        public DrawTypeAOnSpecialUnitDeathModifierRuntimeState(DrawTypeAOnSpecialUnitDeathModifierData data,
            int amountToDraw) : base(data)
        {
            _amountToDraw = amountToDraw;
        }

        protected override IEffect CreateEffect(PlayerController target, ETeam creatorTeam)
        {
            return new DrawTypeAOnSpecialUnitDeathPlayerEffect(target, creatorTeam, DurationType, Duration, EffectData,
                _amountToDraw);
        }
    }
}