using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Player;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="DamageBossOnTypeAPlayModifier"/>.
    /// </summary>
    public class DamageBossOnTypeAPlayRuntimeState : TypedModifierRuntimeState<DamageBossOnTypeAPlayModifier, PlayerController>
    {
        private readonly int _damageAmount;

        public DamageBossOnTypeAPlayRuntimeState(DamageBossOnTypeAPlayModifier data, int damageAmount) : base(data)
        {
            _damageAmount = damageAmount;
        }

        protected override IEffect CreateEffect(PlayerController target, ETeam creatorTeam)
        {
            return new DamageBossOnTypeAPlayEffect(target, creatorTeam, DurationType, Duration, EffectData,
                _damageAmount);
        }
    }
}