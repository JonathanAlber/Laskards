using System;

namespace Gameplay.Flow.Gate
{
    /// <summary>
    /// Disposable handle representing one pending initialization unit.
    /// </summary>
    public readonly struct Token : IDisposable
    {
        private readonly InitBarrier _barrier;
        private readonly int _id;
        private readonly string _reason;

        internal Token(InitBarrier barrier, int id, string reason)
        {
            _barrier = barrier;
            _id = id;
            _reason = reason;
        }

        /// <summary>
        /// Release the token back to the barrier.
        /// </summary>
        public void Dispose() => _barrier?.Release(_id, _reason);
    }
}