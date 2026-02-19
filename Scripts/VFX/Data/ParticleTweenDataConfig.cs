using Systems.Tweening.Core.Data.Parameters;
using UnityEngine;

namespace VFX.Data
{
    /// <summary>
    /// Configuration data for particle tween effects.
    /// </summary>
    [CreateAssetMenu(fileName = "ParticleTweenDataConfig",
        menuName = "ScriptableObjects/VFX/Particle Tween Data Config")]
    public class ParticleTweenDataConfig : ScriptableObject
    {
        [field: Tooltip("Tween settings for when the boss attacks the player")]
        [field: SerializeField] public TweenData BossAttackTweenData { get; private set; }

        [field: Tooltip("Tween settings for when the player attacks the boss")]
        [field: SerializeField] public TweenData PlayerAttackTweenData { get; private set; }

        [field: Tooltip("Tween settings for when a new player effect is spawned")]
        [field: SerializeField] public TweenData EffectSpawnedTweenData { get; private set; }

        [field: Tooltip("Tween settings for when an effect causes the boss to get damaged")]
        [field: SerializeField] public TweenData EffectDamageTweenData { get; private set; }
    }
}