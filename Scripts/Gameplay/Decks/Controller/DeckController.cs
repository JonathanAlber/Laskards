using System.Collections.Generic;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.Decks.Model;
using Gameplay.Decks.View;
using Gameplay.Decks.Zones;
using Systems.Services;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Decks.Controller
{
    /// <summary>
    /// Base controller for any deck of cards (Hand, ActionDeck, UnitDeck, Discard, etc.).
    /// </summary>
    /// <typeparam name="TData">Card data type for initialization.</typeparam>
    /// <typeparam name="TCard">Runtime card component type.</typeparam>
    /// <typeparam name = "TView">View type for this deck.</typeparam>
    public abstract class DeckController<TData, TCard, TView> : MonoBehaviour, ICardZone, ICardOwner, IGameService
        where TData : CardDefinition
        where TCard : CardController
        where TView : DeckView
    {
        public int CardCount => Model.Count;

        public EDeck ZoneType => ZoneTypeInternal;

        public Vector3 BaseScale => View != null ? View.BaseScale : Vector3.one;

        public virtual bool AllowDragging => false;

        /// <summary>
        /// Model for this deck.
        /// </summary>
        public readonly DeckModel<TCard> Model = new();

        /// <summary>
        /// Logical zone type implemented by concrete subclasses.
        /// </summary>
        protected abstract EDeck ZoneTypeInternal { get; }

        /// <summary>
        /// View component for this deck.
        /// </summary>
        protected abstract TView View { get; }

        protected virtual void Awake()
        {
            ((IGameService)this).Register();

            BindModel();
        }

        protected virtual void OnDestroy()
        {
            ((IGameService)this).Deregister();

            UnbindModel();
        }

        private static IReadOnlyList<CardController> CastToCardList(IReadOnlyList<TCard> source)
        {
            int count = source.Count;
            List<CardController> toCardList = new(count);
            for (int i = 0; i < count; i++)
                toCardList.Add(source[i]);

            return toCardList;
        }

        /// <summary>
        /// Returns true if the given card is the visually topmost card of this deck.
        /// </summary>
        public bool IsTopCard(CardController card)
        {
            if (card == null)
                return false;

            return Model.Count != 0 && ReferenceEquals(Model.Cards[^1], card);
        }

        /// <summary>
        /// Adds an existing runtime card to this deck.
        /// </summary>
        public void Add(TCard instance)
        {
            if (instance == null)
            {
                CustomLogger.LogWarning("Attempted to add a null card to the deck.", this);
                return;
            }

            if (Model.Contains(instance))
            {
                CustomLogger.LogWarning("Attempted to add a card that is already contained in this deck.", this);
                return;
            }

            instance.CardInteractionAdapter.OnCardClicked += HandleCardClicked;
            instance.CardInteractionAdapter.SetZone(this);
            instance.CardGate.SetZone(this);

            Model.Add(instance);
        }

        void ICardOwner.AddCard(CardController card) => Add(card as TCard);

        /// <summary>
        /// Removes and returns all cards from this deck in their current order.
        /// </summary>
        public List<TCard> RemoveAll()
        {
            List<TCard> snapshot = new(Model.Cards);
            foreach (TCard card in snapshot)
            {
                card.CardInteractionAdapter.OnCardClicked -= HandleCardClicked;
                card.CardInteractionAdapter.SetZone(null);
                card.CardGate.SetZone(null);
            }

            return Model.RemoveAll();
        }

        public virtual bool TryAcceptPlay(CardController card, out string failReason)
        {
            failReason = string.Empty;
            return false;
        }

        public virtual void OnHoverEnter(CardController card) => View?.OnHoverEnter(card);

        public virtual void OnHoverExit(CardController card) => View?.OnHoverExit(card);

        public virtual void OnDragStarted(CardController card) { }

        public virtual void ReturnCard(CardController card) { }

        public virtual void OnCardPlayed(CardController card) { }

        /// <summary>
        /// Removes a specific card from this deck.
        /// </summary>
        protected void Remove(TCard instance)
        {
            if (instance == null)
            {
                CustomLogger.LogWarning("Attempted to remove a null card from the deck.", this);
                return;
            }

            if (!Model.Contains(instance))
            {
                CustomLogger.LogWarning("Attempted to remove a card that is not contained in this deck.", this);
                return;
            }

            instance.CardInteractionAdapter.OnCardClicked -= HandleCardClicked;
            instance.CardInteractionAdapter.SetZone(null);
            instance.CardGate.SetZone(null);
            Model.Remove(instance);
        }

        /// <summary>
        /// Returns the top card without removing it.
        /// </summary>
        protected TCard Peek() => Model.TryPeek(out TCard c) ? c : null;

        /// <summary>
        /// Called whenever a card is successfully added to this deck.
        /// </summary>
        protected virtual void OnCardAdded(TCard card) { }

        /// <summary>
        /// Called whenever a card is successfully removed from this deck.
        /// </summary>
        protected virtual void OnCardRemoved(TCard card) { }

        /// <summary>
        /// Called after shuffling; controllers can react to the new order.
        /// </summary>
        protected virtual void OnDeckShuffled() { }

        /// <summary>
        /// Handles click events from cards.
        /// </summary>
        protected virtual void HandleCardClicked(CardController cardClickController) { }

        private void BindModel()
        {
            Model.OnCardAdded += OnModelCardAdded;
            Model.OnCardRemoved += OnModelCardRemoved;
            Model.OnShuffled += OnModelShuffled;
            Model.OnCleared += OnModelCleared;
        }

        private void UnbindModel()
        {
            Model.OnCardAdded -= OnModelCardAdded;
            Model.OnCardRemoved -= OnModelCardRemoved;
            Model.OnShuffled -= OnModelShuffled;
            Model.OnCleared -= OnModelCleared;
        }

        private void OnModelCardAdded(TCard card)
        {
            View?.NotifyCardAdded(card);

            OnCardAdded(card);
        }

        private void OnModelCardRemoved(TCard card)
        {
            View?.NotifyCardRemoved(card);

            OnCardRemoved(card);
        }

        private void OnModelShuffled()
        {
            View?.NotifyShuffled(CastToCardList(Model.Cards));

            OnDeckShuffled();
        }

        private void OnModelCleared(List<TCard> clearedCards)
        {
            View?.NotifyCleared(new List<CardController>(clearedCards));
        }
    }
}