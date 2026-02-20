using System.Collections.Generic;
using Interaction.Rotation;
using Systems.Services;
using UnityEngine;
using Utility;

namespace Systems.Tweening.Core
{
    /// <summary>
    /// Observes active tweens and notifies listeners only when a target's rotation actually changes.
    /// Subscribes to <see cref="TweenRunner"/> lifecycle events to track rotation updates.
    /// </summary>
    public sealed class RotationRunner : GameServiceBehaviour
    {
        private readonly List<(ITween tween, Transform target, IRotationNotifiable listener,
            Quaternion lastRotation)> _rotationListeners = new();

        protected override void Awake()
        {
            base.Awake();

            TweenRunner.OnTweenUpdated += HandleTweenUpdated;
            TweenRunner.OnTweenDeregistered += HandleTweenDeregistered;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            TweenRunner.OnTweenUpdated -= HandleTweenUpdated;
            TweenRunner.OnTweenDeregistered -= HandleTweenDeregistered;
        }

        /// <summary>
        /// Registers a rotation listener for the specified tween.
        /// </summary>
        /// <param name="tween">The tween whose rotation will be observed.</param>
        /// <param name="target">The transform being rotated by the tween.</param>
        /// <param name="listener">The listener to notify of rotation changes.</param>
        public void RegisterListener(ITween tween, Transform target, IRotationNotifiable listener)
        {
            if (tween == null || target == null || listener == null)
                return;

            _rotationListeners.Add((tween, target, listener, target.rotation));
        }

        private void HandleTweenUpdated(ITween tween)
        {
            for (int i = _rotationListeners.Count - 1; i >= 0; i--)
            {
                (ITween tweenRef, Transform target, IRotationNotifiable listener, Quaternion lastRotation) entry = _rotationListeners[i];

                if (entry.tweenRef != tween)
                    continue;

                if (entry.target == null)
                {
                    _rotationListeners.RemoveAt(i);
                    continue;
                }

                Quaternion currentRotation = entry.target.rotation;

                if (RotationUtility.ApproximatelyEqual(entry.lastRotation, currentRotation))
                    continue;

                entry.listener.OnRotationChanged(currentRotation);
                _rotationListeners[i] = (entry.tweenRef, entry.target, entry.listener, currentRotation);
            }
        }

        private void HandleTweenDeregistered(ITween tween)
        {
            for (int i = _rotationListeners.Count - 1; i >= 0; i--)
            {
                if (_rotationListeners[i].tween == tween)
                    _rotationListeners.RemoveAt(i);
            }
        }
    }
}
