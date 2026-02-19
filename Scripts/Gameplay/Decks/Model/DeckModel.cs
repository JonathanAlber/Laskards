using System;
using System.Collections.Generic;

namespace Gameplay.Decks.Model
{
    /// <summary>
    /// Pure data model for a deck-like collection of cards.
    /// Owns ordering, add/remove/draw/shuffle and raises events.
    /// </summary>
    /// <typeparam name="TCard">Runtime card component type.</typeparam>
    public sealed class DeckModel<TCard>
    {
        /// <summary>
        /// Raised when a card is added to the model.
        /// </summary>
        public event Action<IReadOnlyList<TCard>> OnCardCountChanged;

        /// <summary>
        /// Raised when a card is added to the model.
        /// </summary>
        public event Action<TCard> OnCardAdded;

        /// <summary>
        /// Raised when a card is removed from the model.
        /// </summary>
        public event Action<TCard> OnCardRemoved;

        /// <summary>
        /// Raised after the list order is shuffled.
        /// </summary>
        public event Action OnShuffled;

        /// <summary>
        /// Raised when the deck is cleared of all cards.
        /// </summary>
        public event Action<List<TCard>> OnCleared;

        /// <summary>
        /// Current number of cards.
        /// </summary>
        public int Count => _cards.Count;

        /// <summary>
        /// Read-only view of the internal card list.
        /// </summary>
        public IReadOnlyList<TCard> Cards => _cards;

        private readonly List<TCard> _cards = new();

        /// <summary>
        /// Returns true if the specified card is contained in the deck.
        /// </summary>
        public bool Contains(TCard card) => _cards.Contains(card);

        /// <summary>
        /// Adds the specified card to the deck (to the end/top).
        /// </summary>
        public void Add(TCard card)
        {
            _cards.Add(card);
            OnCardAdded?.Invoke(card);
            OnCardCountChanged?.Invoke(_cards);
        }

        /// <summary>
        /// Removes the specified card from the deck if present.
        /// </summary>
        public void Remove(TCard card)
        {
            if (!_cards.Remove(card))
                return;

            OnCardRemoved?.Invoke(card);
            OnCardCountChanged?.Invoke(_cards);
        }

        /// <summary>
        /// Removes and returns all cards in current order.
        /// </summary>
        public List<TCard> RemoveAll()
        {
            List<TCard> snapshot = new(_cards);
            _cards.Clear();

            OnCleared?.Invoke(snapshot);
            OnCardCountChanged?.Invoke(_cards);
            return snapshot;
        }

        /// <summary>
        /// Returns the top card without removing it. Returns false if the deck is empty.
        /// </summary>
        public bool TryPeek(out TCard card)
        {
            if (_cards.Count == 0)
            {
                card = default;
                return false;
            }

            card = _cards[^1];
            return true;
        }

        /// <summary>
        /// Draws up to 'amount' cards from the deck, without removing them.
        /// </summary>
        public bool TryDraw(int amount, out List<TCard> drawn)
        {
            drawn = new List<TCard>();

            int count = _cards.Count;
            if (count == 0 || amount <= 0)
                return false;

            int actualAmount = Math.Min(amount, count);

            for (int i = count - 1; i >= count - actualAmount; i--)
                drawn.Add(_cards[i]);

            return drawn.Count > 0;
        }

        /// <summary>
        /// Fisher-Yates shuffle using a caller-provided random-range function.
        /// </summary>
        /// <param name="randomRange">Function like UnityEngine.Random.Range(min, maxExclusive).</param>
        public void Shuffle(Func<int, int, int> randomRange)
        {
            int count = _cards.Count;

            for (int i = 0; i < count; i++)
            {
                int j = randomRange(i, count);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }

            OnShuffled?.Invoke();
        }
    }
}