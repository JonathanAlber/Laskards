using Gameplay.CardExecution;
using Gameplay.Cards.Effects;

namespace Gameplay.Cards.Modifier
{
    /// <summary>
    /// Non-generic interface for an entity that can have effects applied to it.
    /// </summary>
    public interface IModifiableBase
    {
        /// <summary>
        /// Adds an effect to the modifiable entity.
        /// </summary>
        /// <param name="effect">The effect to add.</param>
        void AddEffect(IEffect effect);

        /// <summary>
        /// Removes an effect from the modifiable entity.
        /// </summary>
        /// <param name="effect">The effect to remove.</param>
        void RemoveEffect(IEffect effect);

        /// <summary>
        /// Ticks all active effects, allowing them to update their state or duration.
        /// </summary>
        void TickEffects();

        /// <summary>
        /// Checks if the maximum number of effects has been reached on this modifiable entity.
        /// </summary>
        /// <returns><c>true</c> if the maximum number of effects has been reached; otherwise, <c>false</c>.</returns>
        bool HasMaxEffectsReached();

        /// <summary>
        /// Validates if the player of the specified team can apply effects to this modifiable entity.
        /// </summary>
        /// <param name="team">The team of the player attempting to apply effects.</param>
        /// <returns><c>true</c> if the player can apply effects; otherwise, <c>false</c>.</returns>
        bool ValidatePlayer(ETeam team);
    }
}