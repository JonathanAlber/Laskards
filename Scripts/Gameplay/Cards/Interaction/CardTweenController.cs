using System;
using Systems.Services;
using Systems.Tweening.Components.System;
using Systems.Tweening.Core;
using Systems.Tweening.Core.Data;
using Systems.Tweening.Core.Data.Parameters;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Gameplay.Cards.Interaction
{
    /// <summary>
    /// Animates card transforms using <see cref="TweenFX"/>.
    /// Supports caching active tweens per card so new tweens overwrite old ones cleanly.
    /// Uses struct-based tween parameters for clean editor visualization.
    /// </summary>
    public sealed class CardTweenController : TweenController<CardController>
    {
        [Header("Animation Defaults")]
        [SerializeField, Tooltip("Default duration for move tweens (seconds).")]
        private float moveDuration = 0.45f;

        [SerializeField, Tooltip("Default duration for rotation tweens (seconds).")]
        private float rotateDuration = 0.35f;

        [SerializeField, Tooltip("Default duration for scale tweens (seconds).")]
        private float scaleDuration = 0.40f;

        [SerializeField, Tooltip("Default easing used for all tweens.")]
        private EEasingType easing = EEasingType.EaseOutBack;

        /// <summary>
        /// Tweens position, rotation, and scale together using struct-based data.
        /// Automatically applies default controller values if any struct field is not provided.
        /// </summary>
        /// <param name="card">The card to animate.</param>
        /// /// <param name = "targetPosition">The target local position.</param>
        /// <param name="targetRotation">The target local rotation (Euler angles).</param>
        /// <param name="targetScale">The target local scale.</param>
        /// <param name="moveData">Optional move tween data (uses defaults if not set).</param>
        /// <param name="rotateData">Optional rotation tween data (uses defaults if not set).</param>
        /// <param name="scaleData">Optional scale tween data (uses defaults if not set).</param>
        /// <param name="markCardTransitioning">If true, triggers CardGate transition callbacks.</param>
        /// <param name="onComplete">Optional callback invoked when all tweens complete.</param>
        public void TweenCardTo(CardController card, Vector3 targetPosition, Vector3 targetRotation,
            Vector3 targetScale, TweenData? moveData = null, TweenData? rotateData = null,
            TweenData? scaleData = null, bool markCardTransitioning = true, Action onComplete = null)
        {
            if (card == null)
            {
                onComplete?.Invoke();
                return;
            }

            card.CardGate.FinishTransition();

            KillTweens(card);

            bool isUI = card.transform is RectTransform;

            if (markCardTransitioning)
                card.CardGate.StartTransition();

            TweenData move = moveData ?? new TweenData(duration: moveDuration, easing: easing, delay: 0f);
            TweenData rot = rotateData ?? new TweenData(duration: rotateDuration, easing: easing, delay: 0f);
            TweenData scl = scaleData ?? new TweenData(duration: scaleDuration, easing: easing, delay: 0f);

            TweenBase moveTween = TweenFX.MoveTo(card.transform, targetPosition, move, isUI);
            TweenBase rotTween = TweenFX.RotateTo(card.transform, targetRotation, rot, isUI);
            TweenBase sclTween = TweenFX.ScaleTo(card.transform, targetScale, scl);

            if (ServiceLocator.TryGet(out RotationRunner runner))
                runner.RegisterListener(rotTween, card.transform, card.CardFlipper);

            RegisterTweens(card, moveTween, rotTween, sclTween);

            int total = 0, finished = 0;
            if (moveTween != null)
                total++;

            if (rotTween != null)
                total++;

            if (sclTween != null)
                total++;

            if (total == 0)
            {
                if (markCardTransitioning)
                    card.CardGate.FinishTransition();

                onComplete?.Invoke();
                return;
            }

            if (moveTween != null)
                moveTween.OnComplete += OnPartComplete;

            if (rotTween != null)
                rotTween.OnComplete += OnPartComplete;

            if (sclTween != null)
                sclTween.OnComplete += OnPartComplete;

            return;

            void OnPartComplete(TweenBase completedTween)
            {
                finished++;
                if (finished < total)
                    return;

                if (markCardTransitioning)
                    card.CardGate.FinishTransition();

                onComplete?.Invoke();
                completedTween.OnComplete -= OnPartComplete;
                KillTweens(card);
            }
        }

        /// <summary>
        /// Tweens position, rotation, and scale together using struct-based data.
        /// Automatically applies default controller values if any struct field is not provided.
        /// </summary>
        /// <param name="card">The card to animate.</param>
        /// /// <param name = "targetPosition">The target local position.</param>
        /// <param name="targetRotation">The target local rotation (Euler angles).</param>
        /// <param name="targetScale">The target local scale.</param>
        /// <param name="moveData">Optional move tween data (uses defaults if not set).</param>
        /// <param name="rotateData">Optional rotation tween data (uses defaults if not set).</param>
        /// <param name="scaleData">Optional scale tween data (uses defaults if not set).</param>
        /// <param name="markCardTransitioning">If true, triggers CardGate transition callbacks.</param>
        /// <param name="onComplete">Optional callback invoked when all tweens complete.</param>
        public void TweenCardTo(CardController card, Vector3 targetPosition, Quaternion targetRotation,
            Vector3 targetScale, TweenData? moveData = null, TweenData? rotateData = null,
            TweenData? scaleData = null, bool markCardTransitioning = true, Action onComplete = null)
        {
            TweenCardTo(card, targetPosition, targetRotation.eulerAngles, targetScale,
                moveData, rotateData, scaleData, markCardTransitioning, onComplete);
        }

        /// <summary>
        /// Tweens only the card's local position.
        /// </summary>
        public void MoveCardTo(CardController card, Vector3 targetPosition, TweenData? moveData = null,
            bool markCardTransitioning = true, Action onComplete = null)
        {
            if (card == null)
            {
                onComplete?.Invoke();
                return;
            }

            card.CardGate.FinishTransition();

            KillTweens(card);

            bool isUI = card.transform is RectTransform;

            if (markCardTransitioning)
                card.CardGate.StartTransition();

            TweenData move = moveData ?? new TweenData(duration: moveDuration, easing: easing, delay: 0f);

            TweenBase moveTween = TweenFX.MoveTo(card.transform, targetPosition, move, isUI);

            RegisterTweens(card, moveTween);

            if (moveTween == null)
            {
                if (markCardTransitioning)
                    card.CardGate.FinishTransition();

                onComplete?.Invoke();
                return;
            }

            moveTween.OnComplete += delegate
            {
                if (markCardTransitioning)
                    card.CardGate.FinishTransition();

                KillTweens(card);
                onComplete?.Invoke();
            };
        }

        /// <summary>
        /// Tweens only the card's local rotation.
        /// </summary>
        public void RotateCardTo(CardController card, Vector3 targetRotation, TweenData? rotateData = null,
            bool markCardTransitioning = true, Action onComplete = null)
        {
            if (card == null)
            {
                onComplete?.Invoke();
                return;
            }

            card.CardGate.FinishTransition();

            KillTweens(card);

            bool isUI = card.transform is RectTransform;

            if (markCardTransitioning)
                card.CardGate.StartTransition();

            TweenData rot = rotateData ?? new TweenData(duration: rotateDuration, easing: easing, delay: 0f);

            TweenBase rotTween = TweenFX.RotateTo(card.transform, targetRotation, rot, isUI);

            if (ServiceLocator.TryGet(out RotationRunner runner))
                runner.RegisterListener(rotTween, card.transform, card.CardFlipper);

            RegisterTweens(card, rotTween);

            if (rotTween == null)
            {
                if (markCardTransitioning)
                    card.CardGate.FinishTransition();

                onComplete?.Invoke();
                return;
            }

            rotTween.OnComplete += delegate
            {
                if (markCardTransitioning)
                    card.CardGate.FinishTransition();

                onComplete?.Invoke();
                KillTweens(card);
            };
        }

        /// <summary>
        /// Tweens only the card's local rotation.
        /// </summary>
        public void RotateCardTo(CardController card, Quaternion targetRotation, TweenData? rotateData = null,
            bool markCardTransitioning = true, Action onComplete = null)
        {
            RotateCardTo(card, targetRotation.eulerAngles, rotateData, markCardTransitioning, onComplete);
        }

        /// <summary>
        /// Tweens only the card's local scale.
        /// </summary>
        public void ScaleCardTo(CardController card, Vector3 targetScale, TweenData? scaleData = null,
            bool markCardTransitioning = true, Action onComplete = null)
        {
            if (card == null)
            {
                onComplete?.Invoke();
                return;
            }

            card.CardGate.FinishTransition();

            KillTweens(card);

            if (markCardTransitioning)
                card.CardGate.StartTransition();

            TweenData scl = scaleData ?? new TweenData(duration: scaleDuration, easing: easing, delay: 0f);

            TweenBase sclTween = TweenFX.ScaleTo(card.transform, targetScale, scl);
            RegisterTweens(card, sclTween);

            if (sclTween == null)
            {
                if (markCardTransitioning)
                    card.CardGate.FinishTransition();

                onComplete?.Invoke();
                return;
            }

            sclTween.OnComplete += delegate
            {
                if (markCardTransitioning)
                    card.CardGate.FinishTransition();

                onComplete?.Invoke();
                KillTweens(card);
            };
        }
    }
}