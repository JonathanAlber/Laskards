using Gameplay.Decks.Controller;

namespace Gameplay.Cards.Data
{
    /// <summary>
    /// Describes which <see cref="DeckController{TData, TCard, TView}"/>
    /// the card is currently contained within.
    /// </summary>
    public enum EDeck : byte
    {
        None = 0,
        ActionDeck = 1,
        UnitDeck = 2,
        Hand = 3,
        Discard = 4
    }
}