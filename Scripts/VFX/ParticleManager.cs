using System.Collections.Generic;
using Systems.ObjectPooling;
using Systems.Services;
using Systems.Tweening.Core;
using Systems.Tweening.Core.Data.Parameters;
using UnityEngine;
using Utility.Collections;
using Utility.Logging;
using VFX.Data;
// ReSharper disable MemberCanBePrivate.Global

namespace VFX
{
    /// <summary>
    /// Enables starting and stopping of particle effects throughout the game.
    /// </summary>
    public class ParticleManager : GameServiceBehaviour
    {
        [Header("Object Pool")]

        [Tooltip("Mapping of particle effect types to their corresponding prefabs.")]
        [SerializeField] private SerializableDictionary<EParticleEffectType, PooledParticle> particleEffectPrefabs;

        [Tooltip("Parent transform under which pooled particle effect instances are organized.")]
        [SerializeField] private Transform particleEffectPoolParent;

        private readonly List<PooledParticle> _activeParticles = new();
        private readonly Dictionary<TweenBase, MovingParticleContext> _tweenToParticleMap = new();
        private readonly Dictionary<EParticleEffectType, HashSetObjectPool<PooledParticle>> _pools = new();

        protected override void Awake()
        {
            base.Awake();

            foreach ((EParticleEffectType effectType, PooledParticle prefab) in particleEffectPrefabs)
            {
                if (prefab == null)
                {
                    CustomLogger.LogError($"Particle effect prefab for {effectType} is null.", this);
                    continue;
                }

                HashSetObjectPool<PooledParticle> pool = new(prefab, particleEffectPoolParent);
                _pools[effectType] = pool;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (PooledParticle particle in _activeParticles)
                particle.OnFinished -= OnParticleSystemFinished;

            foreach (TweenBase tween in _tweenToParticleMap.Keys)
                tween.OnComplete -= OnMoveTweenComplete;
        }

        /// <summary>
        /// Plays the specified particle effect at the given position.
        /// </summary>
        /// <param name="effectType">The type of particle effect to play.</param>
        /// <param name="position">The world position to play the effect at.</param>
        /// <param name = "withChildren">Whether to play child particle systems as well.</param>
        public PooledParticle PlayParticleEffect(EParticleEffectType effectType, Vector3 position,
            bool withChildren = true)
        {
            if (!TryAllocate(effectType, out PooledParticle particle))
                return null;

            particle.transform.position = position;
            particle.ParticleSystem.Play(withChildren);

            particle.OnFinished += OnParticleSystemFinished;
            _activeParticles.Add(particle);

            return particle;
        }

        /// <summary>
        /// Stops the specified particle effect and returns it to its pool.
        /// </summary>
        /// <param name="system">The particle system to stop.</param>
        /// <param name = "withChildren">Whether to stop child particle systems as well.</param>
        /// <param name="stopBehavior">The stop behavior for the particle system.
        /// Defining whether to clear existing particles or let them finish.</param>
        public void StopParticleEffect(PooledParticle system, bool withChildren = true,
            ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmittingAndClear)
        {
            if (system == null)
            {
                CustomLogger.LogWarning("Cannot stop a null particle system.", this);
                return;
            }

            if (!_pools.TryGetValue(system.ParticleEffectType, out HashSetObjectPool<PooledParticle> pool))
            {
                CustomLogger.LogWarning("The provided particle system does not belong to any pool.", this);
                return;
            }

            system.ParticleSystem.Stop(withChildren, stopBehavior);
            pool.Release(system);
        }

        /// <summary>
        /// Plays a particle effect, tweens it from start to end, and optionally chains another effect at the end position.
        /// Both effects are auto-released on completion.
        /// </summary>
        public PooledParticle PlayParticleEffectMoving(EParticleEffectType moveEffectType, Vector3 startPosition,
            Vector3 endPosition, TweenData moveTweenData, EParticleEffectType chainedEffectType = EParticleEffectType.None)
        {
            if (!TryAllocate(moveEffectType, out PooledParticle moving))
                return null;

            Transform t = moving.transform;
            t.position = startPosition;

            // Start playing immediately?
            moving.ParticleSystem.Play(true);

            TweenBase tween = TweenFX.MoveTo(t, endPosition, moveTweenData);
            if (tween == null)
            {
                CustomLogger.LogWarning($"Failed to tween particle effect {moveEffectType}. Releasing.", this);
                StopParticleEffect(moving);
                return null;
            }

            MovingParticleContext context = new(chainedEffectType, endPosition, moving);
            _tweenToParticleMap[tween] = context;

            tween.OnComplete += OnMoveTweenComplete;
            return moving;
        }

        private void OnMoveTweenComplete(TweenBase completedTween)
        {
            completedTween.OnComplete -= OnMoveTweenComplete;

            if (!_tweenToParticleMap.TryGetValue(completedTween, out MovingParticleContext context))
            {
                CustomLogger.LogWarning("No moving particle context found for completed tween.", this);
                return;
            }

            StopParticleEffect(context.MovingParticle);

            if (context.ChainedEffectType != EParticleEffectType.None)
                PlayParticleEffect(context.ChainedEffectType, context.TargetPosition);
        }

        private void OnParticleSystemFinished(PooledParticle particle)
        {
            particle.OnFinished -= OnParticleSystemFinished;
            StopParticleEffect(particle);
            _activeParticles.Remove(particle);
        }

        private bool TryAllocate(EParticleEffectType effectType, out PooledParticle particle)
        {
            particle = null;
            if (!_pools.TryGetValue(effectType, out HashSetObjectPool<PooledParticle> pool))
            {
                CustomLogger.LogError($"No pool found for particle effect type: {effectType}", this);
                return false;
            }

            particle = pool.Get();
            if (particle != null)
                return true;

            CustomLogger.LogError($"Failed to allocate particle system for effect type: {effectType}", this);
            return false;
        }
    }
}