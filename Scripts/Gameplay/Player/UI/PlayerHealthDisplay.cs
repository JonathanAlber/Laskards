using TMPro;
using UnityEngine;
using Systems.Tweening.Core;
using Systems.Tweening.Core.Data.Parameters;

namespace Gameplay.Player.UI
{
    /// <summary>
    /// Displays the player's current health in the UI.
    /// </summary>
    public class PlayerHealthDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text healthText;

        [Tooltip("The tween data that controls how the health text will animate.")]
        [SerializeField] private TweenData healthTextTweenData;

        private TweenBase _healthFillTween;

        private void Awake() => PlayerController.OnHpChanged += RefreshHealth;

        private void OnDestroy() => PlayerController.OnHpChanged -= RefreshHealth;

        private void RefreshHealth(int currentHealth)
        {
            _healthFillTween?.Stop();
            _healthFillTween = TweenFX.FadeIntTo(
                fromGetter: () => int.Parse(healthText.text),
                setter: value => healthText.text = value.ToString(),
                targetValue: currentHealth,
                data: healthTextTweenData,
                targetObj: this
            );
        }
    }
}