using System.Collections.Generic;
using Gameplay.Cards;
using Gameplay.Cards.Interaction;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Player;
using Systems.Services;
using Systems.Tweening.Core.Data;
using Systems.Tweening.Core.Data.Parameters;
using TMPro;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Decks.View
{
    /// <summary>
    /// Drawable deck view that arranges cards with depth and offset and handles hover effects.
    /// </summary>
    public class DrawableDeckView : DeckView
    {
        [Header("Drawable Deck View")]

        [Tooltip("Text component to display the current card count in the deck.")]
        [SerializeField] private TMP_Text cardCountText;

        [Header("Visual Ordering")]

        [Tooltip("Z offset between cards in this deck. Negative values stack toward camera.")]
        [SerializeField] private float cardDepthOffset = -0.01f;

        [Tooltip("XY offset per card to create a small visible stack when cards overlap in UI.")]
        [SerializeField] private Vector2 cardOffsetPerCard = new(4f, 4f);

        [Header("Hover")]

        [Tooltip("Local position offset to apply when a card is hovered.")]
        [SerializeField] private Vector3 hoverPositionOffset = new(0f, 0.5f);

        [Tooltip("How much to scale up the card when hovered.")]
        [SerializeField] private float hoverScaleMultiplier = 1.1f;

        [Tooltip("Rotation to apply when a card is hovered.")]
        [SerializeField] private Quaternion hoverRotation;

        [Tooltip("Tween data defining how the card moves when hovered.")]
        [SerializeField] private TweenData moveHoverTweenData = new(0.2f, EEasingType.EaseInOut);

        [Tooltip("Tween data defining how the card rotates when hovered.")]
        [SerializeField] private TweenData rotationHoverTweenData = new(0.2f, EEasingType.EaseInOut);

        [Tooltip("Tween data defining how the card scales when hovered.")]
        [SerializeField] private TweenData scaleHoverTweenData = new(0.2f, EEasingType.EaseInOut);

        [Header("Glow Animation")]
        [SerializeField] private float glowIntensity = 10f;

        private readonly Dictionary<CardController, Vector3> _initialPositions = new();

        private bool _cardsGlowing;

        private void Awake() => PlayerController.OnDrawableCardAmountChanged += HandleDrawableCardAmountChanged;

        private void OnDestroy() => PlayerController.OnDrawableCardAmountChanged -= HandleDrawableCardAmountChanged;

        public override void OnHoverEnter(CardController card)
        {
            if (card == null)
                return;

            if (!ServiceLocator.TryGet(out CardTweenController cardTweenController))
                return;

            if (!_initialPositions.ContainsKey(card))
                _initialPositions[card] = card.transform.localPosition;

            Vector3 basePos = _initialPositions[card];
            Vector3 targetPos = basePos + hoverPositionOffset;

            cardTweenController.TweenCardTo(card, targetPos, hoverRotation, BaseScale * hoverScaleMultiplier,
                moveHoverTweenData, rotationHoverTweenData, scaleHoverTweenData, false);
        }

        public override void OnHoverExit(CardController card)
        {
            if (card == null)
                return;

            if (!ServiceLocator.TryGet(out CardTweenController cardTweenController))
                return;

            if (!_initialPositions.TryGetValue(card, out Vector3 targetPos))
                targetPos = card.transform.localPosition;

            cardTweenController.TweenCardTo(card, targetPos, Quaternion.identity, BaseScale,
                markCardTransitioning: false,
                onComplete: () =>
                {
                    _initialPositions.Remove(card);
                });
        }

        /// <summary>
        /// Gets the local position for a card at the given index in the stack.
        /// </summary>
        /// <param name="index">The index of the card in the stack.</param>
        /// <returns><c>Vector3</c> local position for the card.</returns>
        public Vector3 GetLocalStackPositionForIndex(int index)
        {
            return new Vector3(
                index * cardOffsetPerCard.x,
                index * cardOffsetPerCard.y,
                index * cardDepthOffset
            );
        }

        /// <summary>
        /// Applies full sorting and positioning to all cards in the deck.
        /// </summary>
        public void ApplyFullSorting()
        {
            int count = CurrentCount;
            for (int i = 0; i < count; i++)
            {
                CardController card = CurrentCards[i];

                Vector3 pos = GetLocalStackPositionForIndex(i);

                if (card.transform is RectTransform rect)
                    rect.anchoredPosition3D = pos;
                else
                    card.transform.localPosition = pos;

                card.transform.SetSiblingIndex(i);
            }
        }

        protected override void OnModelCardAdded(CardController card)
        {
            base.OnModelCardAdded(card);

            ApplyFullSorting();
            UpdateCardCountDisplay();

            if (_cardsGlowing)
                SetCardGlow(card, true);
        }

        protected override void OnModelCardRemoved(CardController card)
        {
            _initialPositions.Remove(card);

            ApplyFullSorting();

            UpdateCardCountDisplay();

            SetCardGlow(card, false);
        }

        protected override void OnModelShuffled()
        {
            _initialPositions.Clear();

            ApplyFullSorting();
        }

        protected override void OnModelCleared(List<CardController> clearedCards)
        {
            _initialPositions.Clear();

            ApplyFullSorting();

            foreach (CardController card in clearedCards)
                SetCardGlow(card, false);
        }

        private void HandleDrawableCardAmountChanged(int newAmount)
        {
            if (GameFlowSystem.CurrentPhase is not EGamePhase.PlayerDraw and not EGamePhase.PlayerPrePlay)
            {
                _cardsGlowing = false;
                return;
            }

            _cardsGlowing = newAmount > 0;
            foreach (CardController card in CurrentCards)
                SetCardGlow(card, _cardsGlowing);
        }

        private void SetCardGlow(CardController card, bool shouldGlow)
        {
            if (card == null)
            {
                CustomLogger.LogWarning("Attempted to set glow on null card.", this);
                return;
            }

            if (card.View == null)
            {
                CustomLogger.LogWarning("Attempted to set glow on null card view.", this);
                return;
            }

            card.View.SetGlow(shouldGlow, glowIntensity);
        }

        private void UpdateCardCountDisplay()
        {
            cardCountText.text = CurrentCount.ToString();
            cardCountText.gameObject.SetActive(CurrentCount > 0);
        }
    }
}