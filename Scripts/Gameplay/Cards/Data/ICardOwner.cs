using UnityEngine;

namespace Gameplay.Cards.Data
{
    /// <summary>
    /// Runtime owner of <see cref="CardController"/> instances.
    /// </summary>
    public interface ICardOwner
    {
        /// <summary>
        /// The visual resting scale for cards in this owner.
        /// </summary>
        Vector3 BaseScale { get; }

        /// <summary>
        /// Number of cards currently in this deck.
        /// </summary>
        int CardCount { get; }

        /// <summary>
        /// Adds an existing runtime <see cref="CardController"/> instance to this owner.
        /// </summary>
        void AddCard(CardController card);
    }
}