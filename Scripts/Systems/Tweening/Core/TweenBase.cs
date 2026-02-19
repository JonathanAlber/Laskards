using System;

namespace Systems.Tweening.Core
{
    /// <summary>
    /// Base type for all tween instances. Provides lifecycle and completion events.
    /// </summary>
    public abstract class TweenBase : ITween
    {
        /// <summary>
        /// Event invoked when the tween completes.
        /// </summary>
        public event Action<TweenBase> OnComplete;

        /// <summary>
        /// Indicates if the tween is currently running.
        /// </summary>
        public abstract bool IsRunning { get; }

        /// <summary>
        /// Indicates if the tween has completed.
        /// </summary>
        public abstract bool IsCompleted { get; }

        /// <summary>
        /// Starts the tween.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the tween.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Advances the tween by the given delta time.
        /// </summary>
        /// <param name="deltaTime">Time in seconds since the last tick.</param>
        public abstract void Tick(float deltaTime);

        /// <summary>
        /// Invokes the completion event.
        /// </summary>
        protected void Complete() => OnComplete?.Invoke(this);
    }
}