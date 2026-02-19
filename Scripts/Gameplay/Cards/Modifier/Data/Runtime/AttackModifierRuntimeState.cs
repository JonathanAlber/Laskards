using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.StatLayers.Units;
using Gameplay.Units;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="AttackBonusModifierData"/>.
    /// </summary>
    public class AttackModifierRuntimeState : TypedModifierRuntimeState<AttackBonusModifierData, UnitController>
    {
        private readonly int _attackIncrease;

        public AttackModifierRuntimeState(AttackBonusModifierData data, int attackIncrease) : base(data)
        {
            _attackIncrease = attackIncrease;
        }

        protected override IEffect CreateEffect(UnitController target, ETeam creatorTeam)
        {
            return new AttackBonusUnitEffect(target, creatorTeam, DurationType, Duration, EffectData,
                new AddDamageLayer(_attackIncrease), _attackIncrease);
        }
    }
}