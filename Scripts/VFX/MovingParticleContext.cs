using UnityEngine;
using VFX.Data;

namespace VFX
{
    /// <summary>
    /// Context information for a moving particle effect.
    /// </summary>
    public struct MovingParticleContext
    {
        /// <summary>
        /// The type of particle effect to play once the moving particle reaches its target.
        /// </summary>
        public readonly EParticleEffectType ChainedEffectType;

        /// <summary>
        /// The target position the particle is moving towards.
        /// </summary>
        public readonly Vector3 TargetPosition;

        /// <summary>
        /// The moving particle instance.
        /// </summary>
        public readonly PooledParticle MovingParticle;

        public MovingParticleContext(EParticleEffectType chainedEffectType, Vector3 targetPosition,
            PooledParticle movingParticle)
        {
            ChainedEffectType = chainedEffectType;
            TargetPosition = targetPosition;
            MovingParticle = movingParticle;
        }
    }
}