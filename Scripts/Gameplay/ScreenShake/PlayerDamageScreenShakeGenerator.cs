using Gameplay.Player;
using UnityEngine;

namespace Gameplay.ScreenShake
{
    /// <summary>
    /// Generates screen shake effects in response to the player taking damage.
    /// </summary>
    public class PlayerDamageScreenShakeGenerator : BaseScreenShakeGenerator
    {
        [Tooltip("Multiplier for the screen shake intensity based on damage taken.")]
        [SerializeField] private float damageMultiplier = 0.1f;

        private void OnEnable() => PlayerController.OnPlayerDamaged += HandleDamage;

        private void OnDisable() => PlayerController.OnPlayerDamaged -= HandleDamage;

        private void HandleDamage(int damage)
        {
            float shakeMultiplier = damage * damageMultiplier + multiplier;
            ImpulseSourceWrapper.GenerateShake(transform.position, shakeMultiplier);
        }
    }
}