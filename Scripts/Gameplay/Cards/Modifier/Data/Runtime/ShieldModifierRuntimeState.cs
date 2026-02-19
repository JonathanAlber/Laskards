using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Units;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="ShieldModifierData"/>.
    /// </summary>
    public class ShieldModifierRuntimeState : TypedModifierRuntimeState<ShieldModifierData, UnitController>
    {
        public ShieldModifierRuntimeState(ShieldModifierData data) : base(data) {}

        protected override IEffect CreateEffect(UnitController target, ETeam creatorTeam)
        {
            ShieldUnitEffect effect = new(target, creatorTeam, DurationType, Duration, EffectData);
            return effect;
        }
    }
}