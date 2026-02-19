using Systems.Services;
using UnityEngine;

namespace Gameplay.Flow.Gate
{
    /// <summary>
    /// Persistent owner of the <see cref="InitBarrier"/> instance.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class InitGateHub : GameServiceBehaviour
    {
        public InitBarrier Barrier { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            Barrier = new InitBarrier();
        }
    }
}