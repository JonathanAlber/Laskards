using System;
using System.Collections.Generic;
using System.Threading;
using Utility.Logging;

namespace Gameplay.Flow.Gate
{
    /// <summary>
    /// A gate that blocks gameplay start until all acquired tokens are released.
    /// Steps acquire a token when they begin work and release it when done.
    /// </summary>
    public sealed class InitBarrier
    {
        /// <summary>
        /// Raised when the barrier becomes ready (no pending tokens).
        /// </summary>
        public static event Action OnBecameReady;

        /// <summary>
        /// True if there are no pending tokens.
        /// </summary>
        public bool IsReady => _pendingCount <= 0;

        private readonly HashSet<int> _activeTokens = new();

        private int _pendingCount;
        private int _nextTokenId = 1;

        /// <summary>
        /// Acquire a token that represents a unit of initialization work.
        /// The returned handle must be disposed exactly once.
        /// </summary>
        public Token Acquire(string reason = null)
        {
            int tokenId;
            lock (_activeTokens)
            {
                tokenId = _nextTokenId++;
                _activeTokens.Add(tokenId);
            }

            int newCount = Interlocked.Increment(ref _pendingCount);

            CustomLogger.Log($"Acquire token {tokenId}: {reason}. Pending={newCount}", null);

            return new Token(this, tokenId, reason);
        }

        /// <summary>
        /// Release a previously acquired token.
        /// </summary>
        /// <param name="tokenId">The token ID to release.</param>
        /// <param name="reason"></param>
        public void Release(int tokenId, string reason)
        {
            bool removed;
            lock (_activeTokens)
                removed = _activeTokens.Remove(tokenId);

            if (!removed)
            {
                CustomLogger.LogWarning($"Token {tokenId} release ignored (already released?). Reason={reason}", null);
                return;
            }

            int newCount = Interlocked.Decrement(ref _pendingCount);

            if (newCount == 0)
                OnBecameReady?.Invoke();

            CustomLogger.Log($"Release token {tokenId}: {reason}. Pending={newCount}", null);
        }
    }
}