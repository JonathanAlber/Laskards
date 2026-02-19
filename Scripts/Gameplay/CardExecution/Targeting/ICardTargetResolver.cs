using Gameplay.Board;
using Gameplay.Cards;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;

namespace Gameplay.CardExecution.Targeting
{
    /// <summary>
    /// Resolves a play target for a given card, abstracting input.
    /// </summary>
    public interface ICardTargetResolver
    {
        /// <summary>
        /// Tries to resolve a play target for the provided card.
        /// For <see cref="ActionCardController"/> it should return an <see cref="IModifiableBase"/>.
        /// For <see cref="UnitCardController"/> it should return a <see cref="Tile"/>.
        /// </summary>
        /// <param name="cardModel">The card model to resolve a target for.</param>
        /// <param name="target">The resolved target modifiable.</param>
        /// <returns><c>true</c> if a target could be resolved; otherwise, <c>false</c>.</returns>
        bool TryResolveTarget(CardModel cardModel, out IModifiableBase target);
    }
}