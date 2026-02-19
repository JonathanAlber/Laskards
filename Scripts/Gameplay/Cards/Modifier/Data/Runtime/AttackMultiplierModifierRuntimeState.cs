using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.StatLayers.Units;
using Gameplay.Units;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="AttackMultiplierModifierData"/>.
    /// </summary>
    public class AttackMultiplierModifierRuntimeState
        : TypedModifierRuntimeState<AttackMultiplierModifierData, UnitController>
    {
        private readonly float _attackMultiplier;

        public AttackMultiplierModifierRuntimeState(AttackMultiplierModifierData data, float attackMultiplier)
            : base(data)
        {
            _attackMultiplier = attackMultiplier;
        }

        protected override IEffect CreateEffect(UnitController target, ETeam creatorTeam)
        {
            return new AttackMultiplierUnitEffect(target, creatorTeam, DurationType, Duration, EffectData,
                new MultiplyDamageLayer(_attackMultiplier), _attackMultiplier);
        }
    }
}