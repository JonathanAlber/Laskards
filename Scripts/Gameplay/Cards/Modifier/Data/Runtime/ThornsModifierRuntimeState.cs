using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Units;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="ThornsModifierData"/>.
    /// </summary>
    public class ThornsModifierRuntimeState : TypedModifierRuntimeState<ThornsModifierData, UnitController>
    {
        private readonly float _damageReflectionPercentage;

        public ThornsModifierRuntimeState(ThornsModifierData data, float damageReflectionPercentage) : base(data)
        {
            _damageReflectionPercentage = damageReflectionPercentage;
        }

        protected override IEffect CreateEffect(UnitController target, ETeam creatorTeam)
        {
            return new ThornsUnitEffect(target, creatorTeam, DurationType, Duration, EffectData,
                _damageReflectionPercentage);
        }
    }
}