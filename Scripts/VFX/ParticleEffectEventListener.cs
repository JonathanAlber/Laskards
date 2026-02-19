using VFX.Data;
using UnityEngine;
using Gameplay.Units;
using Gameplay.Player;
using Utility.Logging;
using Systems.Services;
using Gameplay.Movement;
using Gameplay.Movement.AI;
using Gameplay.Cards.Effects.Display;

namespace VFX
{
    /// <summary>
    /// Listens for particle effect events and triggers
    /// corresponding effects via the <see cref="ParticleManager"/>.
    /// </summary>
    public class ParticleEffectEventListener : MonoBehaviour
    {
        [Header("References")]

        [Tooltip("Transform of the board center in the scene.")]
        [SerializeField] private Transform boardCenterTransform;

        [Tooltip("Transform of the player image in the scene.")]
        [SerializeField] private Transform playerImageTransform;

        [Tooltip("Transform of the boss image in the scene.")]
        [SerializeField] private Transform bossImageTransform;

        [Tooltip("Transform of the player groundline in the scene.")]
        [SerializeField] private Transform playerGroundlineTransform;

        [Tooltip("Transform of the boss groundline in the scene.")]
        [SerializeField] private Transform bossGroundlineTransform;

        private ParticleManager _particleManager;
        private ParticleTweenDataProvider _particleTweenDataProvider;

        private void Awake()
        {
            if (!ServiceLocator.TryGet(out _particleManager))
                return;

            if (!ServiceLocator.TryGet(out _particleTweenDataProvider))
                return;

            if (playerImageTransform == null)
                CustomLogger.LogWarning("Player image transform is not assigned.", this);

            if (bossImageTransform == null)
                CustomLogger.LogWarning("Boss image transform is not assigned.", this);

            if (playerGroundlineTransform == null)
                CustomLogger.LogWarning("Player groundline transform is not assigned.", this);

            if (bossGroundlineTransform == null)
                CustomLogger.LogWarning("Boss groundline transform is not assigned.", this);

            BossAutoMoveController.OnUnitEnteredLastRow += HandleBossUnitEnteredLastRow;
            PlayerMoveController.OnUnitEnteredLastRow += HandlePlayerUnitEnteredLastRow;
            PlayerController.OnEffectAdded += HandlePlayerEffectAdded;
            PlayerEffectDisplay.OnBossDamagedByEffect += HandleBossDamagedByEffect;
        }

        private void OnDestroy()
        {
            BossAutoMoveController.OnUnitEnteredLastRow -= HandleBossUnitEnteredLastRow;
            PlayerMoveController.OnUnitEnteredLastRow -= HandlePlayerUnitEnteredLastRow;
            PlayerController.OnEffectAdded -= HandlePlayerEffectAdded;
            PlayerEffectDisplay.OnBossDamagedByEffect -= HandleBossDamagedByEffect;
        }

        private void HandleBossDamagedByEffect(EffectDisplay effectDisplay)
        {
            Vector3 start = effectDisplay.transform.position;
            Vector3 end = bossImageTransform.position;

            _particleManager.PlayParticleEffectMoving(EParticleEffectType.EffectDamage, start, end,
                _particleTweenDataProvider.Config.EffectDamageTweenData,
                EParticleEffectType.EffectDamageHit);
        }

        private void HandlePlayerEffectAdded(EffectDisplay effectDisplay)
        {
            Vector3 start = boardCenterTransform.position;
            Vector3 end = effectDisplay.transform.position;

            _particleManager.PlayParticleEffectMoving(EParticleEffectType.EffectSpawned, start, end,
                _particleTweenDataProvider.Config.EffectSpawnedTweenData);
        }

        private void HandleBossUnitEnteredLastRow(UnitController unit)
        {
            Vector3 unitPos = unit.transform.position;
            Vector3 start = new(unitPos.x, playerGroundlineTransform.position.y, unitPos.z);
            Vector3 end = playerImageTransform.position;

            _particleManager.PlayParticleEffectMoving(EParticleEffectType.BossAttack, start, end,
                _particleTweenDataProvider.Config.BossAttackTweenData,
                EParticleEffectType.BossAttackHit);
        }

        private void HandlePlayerUnitEnteredLastRow(UnitController unit)
        {
            Vector3 unitPos = unit.transform.position;
            Vector3 start = new(unitPos.x, bossGroundlineTransform.position.y, unitPos.z);
            Vector3 end = bossImageTransform.position;

            _particleManager.PlayParticleEffectMoving(EParticleEffectType.UnitAttack, start, end,
                _particleTweenDataProvider.Config.PlayerAttackTweenData,
                EParticleEffectType.UnitAttackHit);
        }
    }
}