using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Player;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="DamageBossOnPlayByActionTypeModifier"/>.
    /// </summary>
    public class DamageBossOnPlayByTypeRuntimeState
        : TypedModifierRuntimeState<DamageBossOnPlayByActionTypeModifier, PlayerController>
    {
        private readonly EActionCategory _actionCategory;
        private readonly int _damageAmount;

        public DamageBossOnPlayByTypeRuntimeState(DamageBossOnPlayByActionTypeModifier data, EActionCategory actionCategory,
            int damageAmount) : base(data)
        {
            _actionCategory = actionCategory;
            _damageAmount = damageAmount;
        }

        protected override IEffect CreateEffect(PlayerController target, ETeam creatorTeam)
        {
            return new DamageBossOnPlayByTypeEffect(target, creatorTeam, DurationType, Duration, EffectData,
                _actionCategory, _damageAmount);
        }
    }
}