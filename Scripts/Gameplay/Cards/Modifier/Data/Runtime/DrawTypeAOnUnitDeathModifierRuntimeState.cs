using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Player;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for <see cref="DrawTypeAOnUnitDeathModifierData"/>.
    /// </summary>
    public class DrawTypeAOnUnitDeathModifierRuntimeState
        : TypedModifierRuntimeState<DrawTypeAOnUnitDeathModifierData, PlayerController>
    {
        private readonly int _amountToDraw;

        public DrawTypeAOnUnitDeathModifierRuntimeState(DrawTypeAOnUnitDeathModifierData data,
            int amountToDraw) : base(data)
        {
            _amountToDraw = amountToDraw;
        }

        protected override IEffect CreateEffect(PlayerController target, ETeam creatorTeam)
        {
            return new DrawTypeAOnUnitDeathPlayerEffect(target, creatorTeam, DurationType, Duration, EffectData,
                _amountToDraw);
        }
    }
}