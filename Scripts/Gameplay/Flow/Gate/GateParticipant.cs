using Systems.Services;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Flow.Gate
{
    /// <summary>
    /// Base class for components that must complete before gameplay begins.
    /// </summary>
    public abstract class GateParticipant : MonoBehaviour
    {
        private Token _token;
        private bool _hasToken;
        private bool _completed;

        protected virtual string Reason => GetType().Name;

        protected virtual void OnEnable()
        {
            if (_completed)
                return;

            if (!ServiceLocator.TryGet(out InitGateHub hub))
            {
                CustomLogger.LogWarning($"No InitGateHub found. '{Reason}' will not gate.", this);
                return;
            }

            if (hub.Barrier == null)
            {
                CustomLogger.LogWarning($"No Barrier found. '{Reason}' will not gate.", this);
                return;
            }

            _token = hub.Barrier.Acquire(Reason);
            _hasToken = true;

            Begin();
        }

        /// <summary>
        /// Start whatever this participant needs to do (open menu, load data, etc.).
        /// </summary>
        protected abstract void Begin();

        /// <summary>
        /// Optional cleanup after completion.
        /// </summary>
        protected virtual void End() { }

        /// <summary>
        /// Call when this initialization requirement is fulfilled.
        /// Safe to call multiple times.
        /// </summary>
        protected void Complete()
        {
            if (_completed)
                return;

            _completed = true;

            if (_hasToken)
            {
                _token.Dispose();
                _hasToken = false;
            }

            End();
        }
    }
}