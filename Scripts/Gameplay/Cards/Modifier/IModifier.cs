using Gameplay.CardExecution;
using Gameplay.Cards.Effects;

namespace Gameplay.Cards.Modifier
{
    /// <summary>
    /// Interface for a modifier that can create effects and execute actions on a modifiable target.
    /// </summary>
    public interface IModifier
    {
        /// <summary>
        /// Creates an effect for the specified modifiable target.
        /// </summary>
        /// <param name="target">The modifiable target for which the effect is created. E.g., a unit or the player.</param>
        /// <param name = "creatorTeam">The team of the entity creating the effect.</param>
        /// <param name = "effect">The created effect if successful; otherwise, null.</param>
        /// <returns><c>true</c> if the effect was successfully created; otherwise, <c>false</c>.</returns>
        bool TryCreateEffect(IModifiableBase target, ETeam creatorTeam, out IEffect effect);

        /// <summary>
        /// Executes the modifier's action.
        /// </summary>
        void Execute();

        /// <summary>
        /// Validates if the modifier can be applied to the specified target.
        /// </summary>
        /// <param name="target">The modifiable target to validate against.</param>
        bool Validate(IModifiableBase target);

        /// <summary>
        /// Cleans up any resources or state used by the modifier.
        /// </summary>
        void Cleanup();
    }
}