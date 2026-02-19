using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Units;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents a shield effect that prevents the unit from being attacked.
    /// </summary>
    public class ShieldUnitEffect : UnitEffect
    {
        public ShieldUnitEffect(UnitController target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData)
            : base(target, creatorTeam, durationType, duration, effectData) {}

        public override bool CanBeAttacked => false;
    }
}