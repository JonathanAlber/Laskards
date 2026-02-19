using Gameplay.Cards;
using Gameplay.Cards.Model;
using Gameplay.Units;

namespace Gameplay.CardExecution
{
    /// <summary>
    /// Defines an entity capable of playing cards — player, boss, or other AI.
    /// Handles cost validation and resource spending.
    /// </summary>
    public interface ICardPlayer
    {
        /// <summary>
        /// The side this card player belongs to.
        /// </summary>
        ETeam Team { get; }

        /// <summary>
        /// Determines if the card player can afford to play the specified card.
        /// </summary>
        /// <param name="cardModel">The card model to check affordability for.</param>
        /// <returns><c>true</c> if the card can be played; otherwise, <c>false</c>.</returns>
        bool CanPlay(CardModel cardModel);

        /// <summary>
        /// Called when a card has been successfully executed.
        /// Handles cost deduction or any post-play logic.
        /// </summary>
        /// <param name="card">The card that was executed.</param>
        void OnCardExecuted(CardController card);

        /// <summary>
        /// Called when a new unit is spawned by this card player.
        /// </summary>
        /// <param name="unit">The newly spawned unit.</param>
        void OnNewUnitSpawned(UnitController unit);
    }
}