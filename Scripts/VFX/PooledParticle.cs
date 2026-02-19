using System;
using UnityEngine;
using VFX.Data;

namespace VFX
{
    /// <summary>
    /// Instance of a particle effect that is managed via object pooling.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class PooledParticle : MonoBehaviour
    {
        /// <summary>
        /// Event invoked when the particle effect has finished playing.
        /// </summary>
        public event Action<PooledParticle> OnFinished;

        /// <summary>
        /// The type of particle effect this instance represents.
        /// </summary>
        [field: SerializeField] public EParticleEffectType ParticleEffectType { get; private set; }

        /// <summary>
        /// The ParticleSystem component associated with this pooled particle.
        /// </summary>
        public ParticleSystem ParticleSystem { get; private set; }

        private void Awake() => ParticleSystem = GetComponent<ParticleSystem>();

        private void OnParticleSystemStopped() => OnFinished?.Invoke(this);
    }
}