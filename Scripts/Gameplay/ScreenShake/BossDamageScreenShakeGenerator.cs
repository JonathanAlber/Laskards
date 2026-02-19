using Gameplay.Boss;
using UnityEngine;

namespace Gameplay.ScreenShake
{
    /// <summary>
    /// Generates screen shake effects in response to the boss taking damage.
    /// </summary>
    public class BossDamageScreenShakeGenerator : BaseScreenShakeGenerator
    {
        [Tooltip("Multiplier for the screen shake intensity based on damage taken.")]
        [SerializeField] private float damageMultiplier = 0.1f;

        private void OnEnable() => BossModel.OnDamageTaken += HandleDamage;

        private void OnDisable() => BossModel.OnDamageTaken -= HandleDamage;

        private void HandleDamage(int damage)
        {
            float shakeMultiplier = damage * damageMultiplier + multiplier;
            ImpulseSourceWrapper.GenerateShake(transform.position, shakeMultiplier);
        }
    }
}