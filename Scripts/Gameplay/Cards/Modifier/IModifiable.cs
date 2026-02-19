using System.Collections.Generic;
using Gameplay.Cards.Effects;

namespace Gameplay.Cards.Modifier
{
    /// <summary>
    /// Interface for an entity that can have effects applied to it.
    /// </summary>
    /// <typeparam name="T">The type of effect that can be applied to this modifiable entity.</typeparam>
    public interface IModifiable<T> : IModifiableBase where T : IEffect
    {
        /// <summary>
        /// Gets a read-only list of currently active effects on the modifiable entity.
        /// </summary>
        List<T> ActiveEffects { get; }

        /// <summary>
        /// Adds an effect to the modifiable entity.
        /// </summary>
        /// <param name="effect">The effect to add.</param>
        void AddEffect(T effect);

        /// <summary>
        /// Removes an effect from the modifiable entity.
        /// </summary>
        /// <param name="effect">The effect to remove.</param>
        void RemoveEffect(T effect);

        /// <summary>
        /// Clears all effects from the modifiable entity and calls their revert logic.
        /// </summary>
        void ClearEffects();
    }
}