using Gameplay.Cards.Data;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Base class for AI effect snapshots.
    /// </summary>
    /// <typeparam name="TSelf">Type of the derived snapshot class.</typeparam>
    public abstract class AiEffectSnapshot<TSelf> where TSelf : AiEffectSnapshot<TSelf>
    {
        /// <summary>
        /// Type of duration for this effect (temporary or permanent).
        /// </summary>
        public readonly EDurationType DurationType;

        /// <summary>
        /// Remaining duration in turns for temporary effects.
        /// </summary>
        public readonly int RemainingDuration;

        protected AiEffectSnapshot(EDurationType durationType, int remainingDuration)
        {
            DurationType = durationType;
            RemainingDuration = remainingDuration;
        }

        /// <summary>
        /// Indicates whether the effect has expired.
        /// </summary>
        public bool IsExpired => DurationType == EDurationType.Temporary && RemainingDuration <= 0;

        /// <summary>
        /// Ticks the effect, reducing its duration if temporary.
        /// </summary>
        /// <returns>A new snapshot with updated duration.</returns>
        public abstract TSelf Tick();
    }
}