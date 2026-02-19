using Gameplay.Cards.Data;
using Gameplay.StatLayers.Units;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Snapshot of a unit effect for AI evaluation purposes.
    /// </summary>
    public sealed class AiUnitEffectSnapshot : AiEffectSnapshot<AiUnitEffectSnapshot>
    {
        /// <summary>
        /// Pure stat layer this effect applies while active (optional).
        /// </summary>
        public readonly IUnitStatLayer StatLayer;

        /// <summary>
        /// Indicates whether the unit can be attacked while this effect is active.
        /// </summary>
        public readonly bool CanBeAttacked;

        /// <summary>
        /// Percentage of damage reflected back to attackers via thorns while this effect is active.
        /// </summary>
        public readonly float ThornsReflectionPercentage;

        public AiUnitEffectSnapshot(EDurationType durationType, int remainingDuration,
            bool canBeAttacked, IUnitStatLayer unitStatLayer, float thornsReflectionPercentage = 0)
            : base(durationType, remainingDuration)
        {
            StatLayer = unitStatLayer;
            CanBeAttacked = canBeAttacked;
            ThornsReflectionPercentage = thornsReflectionPercentage;
        }

        public override AiUnitEffectSnapshot Tick()
        {
            return DurationType != EDurationType.Temporary
                ? this
                : new AiUnitEffectSnapshot(DurationType, RemainingDuration - 1, CanBeAttacked, StatLayer);
        }
    }
}