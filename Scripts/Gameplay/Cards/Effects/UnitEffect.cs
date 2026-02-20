using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Units;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents an effect that can be applied to a unit in the game.
    /// </summary>
    public class UnitEffect : Effect<UnitController>
    {
        /// <summary>
        /// Determines whether the unit can be attacked while this effect is active.
        /// </summary>
        /// <returns><c>true</c> if the unit can be attacked; otherwise, <c>false</c>.</returns>
        public virtual bool CanBeAttacked => true;

        protected UnitEffect(UnitController target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData)
            : base(target, creatorTeam, durationType, duration, effectData) { }
    }
}