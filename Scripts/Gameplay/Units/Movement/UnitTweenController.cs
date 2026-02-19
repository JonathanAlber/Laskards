using System;
using System.Collections.Generic;
using Systems.Tweening.Components.System;
using Systems.Tweening.Core;
using Systems.Tweening.Core.Data;
using Systems.Tweening.Core.Data.Parameters;
using UnityEngine;

namespace Gameplay.Units.Movement
{
    /// <summary>
    /// Animates unit transforms using <see cref="TweenFX"/>.
    /// Caches active tweens per unit so new tweens overwrite old ones cleanly.
    /// Also tracks transitioning units for external systems to query.
    /// </summary>
    public sealed class UnitTweenController : TweenController<UnitController>
    {
        [Header("Animation Defaults")]
        [SerializeField, Tooltip("Default duration for unit move tweens (seconds).")]
        private float moveDuration = 0.45f;

        [SerializeField, Tooltip("Default easing used for unit move tweens.")]
        private EEasingType easing = EEasingType.EaseOutBack;

        private readonly HashSet<UnitController> _transitioningUnits = new();

        /// <summary>
        /// Tweens only the unit's world position.
        /// </summary>
        /// <param name="unit">The unit to animate.</param>
        /// <param name="targetPosition">The target world position.</param>
        /// <param name="moveData">Optional move tween data (uses defaults if not set).</param>
        /// <param name="markUnitTransitioning">If true, marks the unit as transitioning for the duration of the tween.</param>
        /// <param name="onComplete">Optional callback invoked when the tween completes.</param>
        public void MoveUnitTo(UnitController unit, Vector3 targetPosition, TweenData? moveData = null,
            bool markUnitTransitioning = true, Action onComplete = null)
        {
            if (unit == null)
            {
                onComplete?.Invoke();
                return;
            }

            KillTweens(unit);

            if (markUnitTransitioning)
                StartTransition(unit);

            TweenData move = moveData ?? new TweenData(duration: moveDuration, easing: easing, delay: 0f);

            TweenBase moveTween = TweenFX.MoveTo(unit.transform, targetPosition, move, local: false);

            RegisterTweens(unit, moveTween);

            if (moveTween == null)
            {
                if (markUnitTransitioning)
                    FinishTransition(unit);

                onComplete?.Invoke();
                return;
            }

            moveTween.OnComplete += delegate
            {
                if (markUnitTransitioning)
                    FinishTransition(unit);

                onComplete?.Invoke();
                KillTweens(unit);
            };
        }

        /// <summary>
        /// Returns whether the given unit is currently transitioning (has an active move tween).
        /// </summary>
        public bool IsUnitTransitioning(UnitController unit) => unit != null && _transitioningUnits.Contains(unit);

        private void StartTransition(UnitController unit)
        {
            if (unit == null)
                return;

            _transitioningUnits.Add(unit);
        }

        private void FinishTransition(UnitController unit)
        {
            if (unit == null)
                return;

            _transitioningUnits.Remove(unit);
        }
    }
}