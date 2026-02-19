using TMPro;
using System;
using UnityEngine;
using Gameplay.Cards;
using Utility.Logging;
using Systems.Services;
using Gameplay.Cards.Interaction;
using System.Collections.Generic;
using Systems.Tweening.Core.Data;
using Systems.Tweening.Core.Data.Parameters;

namespace Gameplay.Decks.View
{
    /// <summary>
    /// Visual component for the discard pile, which holds played cards.
    /// </summary>
    public sealed class DiscardPileView : DeckView
    {
        [Header("Drawable Deck View")]

        [Tooltip("Text component to display the current card count in the deck.")]
        [SerializeField] private TMP_Text cardCountText;

        [Tooltip("The canvas in which the discard pile exists.")]
        [SerializeField] private Canvas canvas;

        [Header("Tween Settings")]

        [Tooltip("Tween data defining how the cards move back to their owners.")]
        [SerializeField] private TweenData moveTweenData = new(1.5f, EEasingType.EaseInOut);

        [Tooltip("Tween data defining how cards rotate back to their owners.")]
        [SerializeField] private TweenData rotateTweenData = new(1.5f, EEasingType.EaseInOut);

        [Tooltip("Tween data defining how cards scale back to their owners.")]
        [SerializeField] private TweenData scaleTweenData = new(2f, EEasingType.EaseInOut);

        [Tooltip("Delay between each card's animation starting (in seconds).")]
        [SerializeField] private float batchDelay = 0.075f;

        private void Start() => UpdateCardCountDisplay();

        public override void OnHoverEnter(CardController card) { }

        public override void OnHoverExit(CardController card) { }

        protected override void OnModelCardAdded(CardController card)
        {
            base.OnModelCardAdded(card);

            UpdateCardCountDisplay();
        }

        protected override void OnModelShuffled() { }

        protected override void OnModelCardRemoved(CardController card) => UpdateCardCountDisplay();

        protected override void OnModelCleared(List<CardController> clearedCards) => UpdateCardCountDisplay();

        /// <summary>
        /// Animates all cards returning from the discard pile to their owners.
        /// Uses <see cref="TweenData"/> structs for unified and editor-visible control.
        /// </summary>
        public void AnimateRecycleToOwners(IReadOnlyList<CardController> cards,
            IReadOnlyDictionary<CardController, Vector3> worldStartPositions,
            Action<CardController> onCardArrived = null)
        {
            if (cards == null || cards.Count == 0)
                return;

            if (!ServiceLocator.TryGet(out CardTweenController tweens))
                return;

            UpdateCardCountDisplay();

            for (int i = 0; i < cards.Count; i++)
            {
                CardController card = cards[i];
                if (card == null)
                    continue;

                if (card.CardGate.IsTransitioning)
                {
                    CustomLogger.LogWarning($"Card {card.name} is transitioning; cannot animate recycle to owner.", this);
                    onCardArrived?.Invoke(card);
                    continue;
                }

                if (!worldStartPositions.TryGetValue(card, out Vector3 startWorld))
                {
                    CustomLogger.LogWarning($"Missing start position for card {card.name}.", this);
                    continue;
                }

                float delay = batchDelay * i;

                Action arrived = () => onCardArrived?.Invoke(card);

                if (card.transform is RectTransform rect)
                {
                    Transform parent = rect.parent;
                    Vector3 targetLocal = rect.anchoredPosition3D;

                    Camera canvasCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
                        ? null
                        : canvas != null ? canvas.worldCamera : null;

                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, startWorld);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(parent as RectTransform, screenPoint,
                        canvasCamera, out Vector2 localPoint);

                    rect.anchoredPosition3D = new Vector3(localPoint.x, localPoint.y, targetLocal.z);

                    Vector3 scale = card.InitialOwner?.BaseScale ?? BaseScale;
                    tweens.TweenCardTo(card, targetLocal, Quaternion.identity, scale,
                        moveTweenData.WithDelay(delay), rotateTweenData.WithDelay(delay),
                        scaleTweenData.WithDelay(delay), true, arrived);
                }
                else
                {
                    Vector3 targetWorld = card.transform.position;
                    card.transform.position = startWorld;

                    Vector3 scale = card.InitialOwner?.BaseScale ?? BaseScale;
                    tweens.TweenCardTo(card, targetWorld, Quaternion.identity, scale,
                        moveTweenData.WithDelay(delay), rotateTweenData.WithDelay(delay),
                        scaleTweenData.WithDelay(delay), true, arrived);
                }
            }
        }

        private void UpdateCardCountDisplay()
        {
            cardCountText.text = CurrentCount.ToString();
            cardCountText.gameObject.SetActive(CurrentCount > 0);
        }
    }
}