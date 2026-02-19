using System.Collections.Generic;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.Cards.Interaction;
using Gameplay.Decks.View;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Player;
using Systems.Services;
using Systems.Tweening.Core.Data;
using Systems.Tweening.Core.Data.Parameters;
using UnityEngine;
using Utility;
using Utility.Logging;

namespace Gameplay.Decks.Controller
{
    /// <summary>
    /// Controller for draw-able decks.
    /// </summary>
    public abstract class DrawableDeckController<TData, TCard, TView>
        : DeckController<TData, TCard, TView>, IDiscardRecycleReceiver
        where TData : CardDefinition
        where TCard : CardController
        where TView : DrawableDeckView
    {
        [Header("Drawable View")]
        [Tooltip("Visual component for this deck.")]
        [SerializeField] private TView drawableView;

        [Header("Visual Settings")]
        [Tooltip("The material to be applied to the backside of the cards in this deck.")]
        [SerializeField] private Material cardBacksideMaterial;
        [SerializeField] private Sprite cardBacksideSprite;

        [Header("Animation Settings")]
        [Tooltip("Delay between drawing each card to hand. Used e.g. when drawing through modifiers.")]
        [SerializeField] protected float delayBetweenDraws = 0.2f;

        [Header("Shuffle Animation")]
        [SerializeField] private TweenData shuffleScatterTween = new(0.18f, EEasingType.EaseOutBack);
        [SerializeField] private TweenData shuffleStackTween = new(0.22f, EEasingType.EaseInOut);
        [SerializeField] private float shufflePerCardDelay = 0.02f;
        [SerializeField] private Vector2 shuffleScatterRadius = new(40f, 25f);

        protected override TView View => drawableView;

        public override void OnHoverEnter(CardController card)
        {
            if (GameFlowSystem.CurrentPhase != EGamePhase.PlayerDraw)
                return;

            if (card == null || !IsTopCard(card))
                return;

            if (drawableView != null)
                drawableView.OnHoverEnter(card);
            else
                base.OnHoverEnter(card);
        }

        public override void OnHoverExit(CardController card)
        {
            if (GameFlowSystem.CurrentPhase != EGamePhase.PlayerDraw)
                return;

            if (card == null || !IsTopCard(card))
                return;

            if (card.CardGate.IsTransitioning)
                return;

            if (drawableView != null)
                drawableView.OnHoverExit(card);
            else
                base.OnHoverExit(card);
        }

        /// <summary>
        /// Draws the given number of cards without validating phase or player conditions.
        /// Can be used for effects that force drawing cards.
        /// </summary>
        /// <param name="amount">The number of cards to draw.</param>
        /// <returns><c>true</c> if any cards were drawn; otherwise, <c>false</c>.</returns>
        public bool TryDraw(int amount)
        {
            if (!Model.TryDraw(amount, out List<TCard> drawn))
                return false;

            DrawToHand(drawn);
            return true;
        }

        void IDiscardRecycleReceiver.OnDiscardRecycleCompleted() => ShuffleAndAnimate();

        private void ShuffleAndAnimate()
        {
            if (Model.Count <= 1)
                return;

            Model.Shuffle(Random.Range);

            if (!ServiceLocator.TryGet(out CardTweenController tweens))
            {
                View.ApplyFullSorting();
                return;
            }

            int count = Model.Count;
            for (int i = 0; i < count; i++)
            {
                CardController c = Model.Cards[i];
                int index = i;

                Vector3 finalPos = View.GetLocalStackPositionForIndex(index);

                Vector2 rnd = new(
                    Random.Range(-shuffleScatterRadius.x, shuffleScatterRadius.x),
                    Random.Range(-shuffleScatterRadius.y, shuffleScatterRadius.y)
                );

                Vector3 scatterPos = finalPos + new Vector3(rnd.x, rnd.y, 0f);
                float delay = index * shufflePerCardDelay;

                CoroutineRunner.Instance.RunNextFrame(() =>
                {
                    tweens.MoveCardTo(c, scatterPos, shuffleScatterTween.WithDelay(delay), onComplete: () =>
                    {
                        CoroutineRunner.Instance.RunNextFrame(() =>
                        {
                            tweens.MoveCardTo(c, finalPos, shuffleStackTween, onComplete: () =>
                            {
                                if (index == count - 1)
                                    View.ApplyFullSorting();
                            });
                        });
                    });
                });
            }
        }

        /// <summary>
        /// Creates runtime card objects from data entries and adds them to this deck.
        /// Controller owns instantiation; model remains pure.
        /// </summary>
        protected void InitializeFromDeckData(List<TData> cards)
        {
            if (cards == null)
            {
                CustomLogger.LogWarning("Starter deck cards are null. Cannot initialize deck.", this);
                return;
            }

            RemoveAll();

            foreach (TData data in cards)
            {
                if (data == null)
                {
                    CustomLogger.LogWarning("Encountered null card data in deck '" + GetType().Name + "'. Skipping.", this);
                    continue;
                }

                TCard instance = GetCardInstance(data);
                if (instance == null)
                {
                    CustomLogger.LogWarning("Failed to instantiate card for deck '" + GetType().Name + "'. Skipping.", this);
                    continue;
                }

                instance.transform.SetParent(View != null ? View.CardParent : null);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = BaseScale;
                instance.name = data.Common.DisplayName;

                instance.Initialize(data);
                instance.View.SetBacksideMaterial(cardBacksideMaterial);
                instance.View.SetBacksideSprite(cardBacksideSprite);
                instance.SetInitialOwner(this);

                Add(instance);
            }

            Model.Shuffle(Random.Range);
        }

        /// <summary>
        /// Creates and returns an instance of the card prefab for this deck
        /// using the custom pooling manager for the card type.
        /// </summary>
        /// <returns>The instantiated card.</returns>
        protected abstract TCard GetCardInstance(TData data);

        /// <summary>
        /// Draws the specified cards to the player's hand with animation.
        /// </summary>
        /// <param name="drawn">The list of cards to draw.</param>
        protected void DrawToHand(List<TCard> drawn)
        {
            if (drawn == null || drawn.Count == 0)
            {
                CustomLogger.LogWarning("Attempted to draw an empty or null list of cards to hand.", this);
                return;
            }

            if (!ServiceLocator.TryGet(out HandDeckController handDeck))
                return;

            for (int i = 0; i < drawn.Count; i++)
            {
                int index = i;
                CoroutineRunner.Instance.RunAfterSeconds(() =>
                {
                    TCard card = drawn[index];
                    Remove(card);
                    handDeck.Add(card);
                }, index * delayBetweenDraws);
            }

            if (Model.Count == 0 && ServiceLocator.TryGet(out DiscardPileController discardPile))
                discardPile.RecycleToOriginDecks();
        }

        /// <summary>
        /// Attempts to draw the specified card to the player's hand,
        /// validating the game phase and player conditions.
        /// </summary>
        protected void DrawToHandValidated(TCard card)
        {
            if (GameFlowSystem.CurrentPhase != EGamePhase.PlayerDraw)
                return;

            if (card == null)
            {
                CustomLogger.LogWarning("Attempted to draw a null card from the deck.", this);
                return;
            }

            if (card != Peek())
            {
                CustomLogger.Log($"Attempted to draw card '{card.Model.Common.DisplayName}'" +
                                        " which is not on top of the deck.", this);
                return;
            }

            if (!ServiceLocator.TryGet(out PlayerController playerState))
                return;

            if (!playerState.CanDraw())
                return;

            AddCardToHandImmediate(card);
            playerState.CardDrawn();
        }

        private void AddCardToHandImmediate(TCard card)
        {
            if (!ServiceLocator.TryGet(out HandDeckController handDeck))
                return;

            Remove(card);
            handDeck.Add(card);

            if (Model.Count == 0 && ServiceLocator.TryGet(out DiscardPileController discardPile))
                discardPile.RecycleToOriginDecks();
        }
    }
}