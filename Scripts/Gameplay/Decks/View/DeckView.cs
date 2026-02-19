using System.Collections.Generic;
using Gameplay.Cards;
using UnityEngine;

namespace Gameplay.Decks.View
{
    /// <summary>
    /// Base View for deck visuals: parenting and simple stack sorting.
    /// Controller forwards model events to this view.
    /// </summary>
    public abstract class DeckView : MonoBehaviour
    {
        [Header("Card Instantiation")]

        [Tooltip("Parent transform for instantiated card objects.")]
        [SerializeField] protected Transform cardParent;

        [Header("Scale")]

        [Tooltip("Resting scale for cards in this view.")]
        [SerializeField] private Vector3 baseScale = Vector3.one;

        /// <summary>
        /// Resting scale for cards in this view.
        /// </summary>
        public Vector3 BaseScale => baseScale;

        /// <summary>
        /// Parent for cards (instantiation parent).
        /// </summary>
        public Transform CardParent => cardParent;

        /// <summary>
        /// Current cards snapshot the view is laying out.
        /// </summary>
        protected IReadOnlyList<CardController> CurrentCards => _cards;

        /// <summary>
        /// Current count.
        /// </summary>
        protected int CurrentCount => _cards.Count;

        private readonly List<CardController> _cards = new();

        /// <summary>
        /// Notify view that a card was added at the logical top.
        /// </summary>
        public void NotifyCardAdded(CardController card)
        {
            if (card == null)
                return;

            _cards.Add(card);
            OnModelCardAdded(card);
        }

        /// <summary>
        /// Notify view that a card was removed.
        /// </summary>
        public void NotifyCardRemoved(CardController card)
        {
            if (card == null)
                return;

            _cards.Remove(card);
            OnModelCardRemoved(card);
        }

        /// <summary>
        /// Notify view that the underlying order changed (shuffle).
        /// </summary>
        public void NotifyShuffled(IReadOnlyList<CardController> cards)
        {
            SyncAll(cards);
            OnModelShuffled();
        }

        /// <summary>
        /// Notify view that the deck cleared.
        /// </summary>
        public void NotifyCleared(List<CardController> clearedCards)
        {
            OnModelCleared(clearedCards);
            _cards.Clear();
        }

        /// <summary>
        /// Called when a card in this zone is hovered by the pointer.
        /// </summary>
        public abstract void OnHoverEnter(CardController card);

        /// <summary>
        /// Called when a card in this zone stops being hovered.
        /// </summary>
        public abstract void OnHoverExit(CardController card);

        /// <summary>
        /// Called when a card is removed from the deck.
        /// </summary>
        protected abstract void OnModelCardRemoved(CardController card);

        /// <summary>
        /// Called when the model is shuffled.
        /// </summary>
        protected abstract void OnModelShuffled();

        /// <summary>
        /// Called when the model is cleared.
        /// </summary>
        protected abstract void OnModelCleared(List<CardController> removedCards);

        /// <summary>
        /// Default parenting + full sort when a card enters.
        /// </summary>
        protected virtual void OnModelCardAdded(CardController card)
        {
            card.transform.SetParent(cardParent, true);
            card.name = card.Model != null ? card.Model.Common.DisplayName : card.name;
        }

        private void SyncAll(IReadOnlyList<CardController> cards)
        {
            _cards.Clear();
            _cards.AddRange(cards);
        }
    }
}