using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Units;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="HealthBonusModifierData"/>.
    /// </summary>
    public class HealthBonusModifierRuntimeState : TypedModifierRuntimeState<HealthBonusModifierData, UnitController>
    {
        private readonly int _gainedHealth;
        private UnitController _targetUnit;

        public HealthBonusModifierRuntimeState(HealthBonusModifierData data, int gainedHealth) : base(data)
        {
            _gainedHealth = gainedHealth;
        }

        public override void Execute()
        {
            base.Execute();
            _targetUnit.Model.GainHealth(_gainedHealth);
        }

        protected override IEffect CreateEffect(UnitController target, ETeam creatorTeam)
        {
            _targetUnit = target;
            return new HealthBonusUnitEffect(target, creatorTeam, DurationType, Duration, EffectData, _gainedHealth);
        }
    }
}