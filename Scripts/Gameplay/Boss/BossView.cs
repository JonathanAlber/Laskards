using Gameplay.Flow;
using Gameplay.Flow.Data;
using Systems.Tweening.Components.System;
using Systems.Tweening.Components.UITweens;
using Systems.Tweening.Core;
using Systems.Tweening.Core.Data.Parameters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;

namespace Gameplay.Boss
{
    /// <summary>
    /// Displays boss-related visual information.
    /// </summary>
    public class BossView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private GameObject bossRoot;

        [SerializeField] private Image armorFillImage;
        [SerializeField] private Image healthFillImage;

        [Header("Tweens")]
        [Tooltip("Visual tween for glow effect on the boss UI representation.")]
        [SerializeField] private FadeTweenInit glowVisualFadeTween;

        [Tooltip("Tween group for the lock icon fade animation.")]
        [SerializeField] private TweenGroup lockIconTweenGroup;

        [Header("Tween Data")]
        [Tooltip("The tween data that controls how the armor fill highlight will animate.")]
        [SerializeField] private TweenData armorFillTweenData;

        [Tooltip("The tween data that controls how the health fill highlight will animate.")]
        [SerializeField] private TweenData healthFillTweenData;

        [Tooltip("The tween data that controls how the health text will animate.")]
        [SerializeField] private TweenData healthTextTweenData;

        private BossModel _bossModel;
        private TweenBase _healthTextTween;
        private TweenBase _armorFillTween;
        private TweenBase _healthFillTween;

        private bool _isBossTurn;

        private void Awake() => GameFlowSystem.OnPhaseStarted += HandlePhaseStarted;

        private void OnDestroy()
        {
            GameFlowSystem.OnPhaseStarted -= HandlePhaseStarted;

            if (_bossModel == null)
                return;

            _bossModel.OnHealthChanged -= UpdateHealthDisplay;
            BossModel.OnAggressive -= HandleBossAggressive;
        }

        /// <summary>
        /// Initializes the <see cref="BossView"/> with the given <see cref="BossModel"/>.
        /// </summary>
        public void Initialize(BossModel bossModel)
        {
            _bossModel = bossModel;

            UpdateHealthDisplay();

            _bossModel.OnHealthChanged += UpdateHealthDisplay;
            BossModel.OnAggressive += HandleBossAggressive;
        }

        private void UpdateHealthDisplay()
        {
            if (_bossModel == null)
            {
                CustomLogger.LogWarning($"{nameof(BossModel)} is null in {nameof(BossView)} during health update.", this);
                return;
            }

            int currentHealth = _bossModel.CurrentHp;
            int maxHealth = _bossModel.MaxHealth;

            int maxArmorHealth = Mathf.CeilToInt(maxHealth * _bossModel.AggressiveThreshold);
            int armorHealth = currentHealth - maxArmorHealth;

            // Health text
            _healthTextTween?.Stop();
            _healthTextTween = TweenFX.FadeIntTo(
                fromGetter: () => int.Parse(healthText.text),
                setter: value => healthText.text = value.ToString(),
                targetValue: currentHealth,
                data: healthTextTweenData,
                targetObj: this
            );

            if (_bossModel.IsAggressive)
            {
                // Health fill
                int maxNormalHealth = maxHealth - maxArmorHealth;
                float normalizedHealth = maxNormalHealth > 0
                    ? (float)currentHealth / maxNormalHealth
                    : 0f;

                _healthFillTween?.Stop();
                _healthFillTween = TweenFX.FadeFloatTo(
                    fromGetter: () => healthFillImage.fillAmount,
                    setter: value => healthFillImage.fillAmount = value,
                    targetValue: normalizedHealth,
                    data: healthFillTweenData,
                    targetObj: this
                );
            }
            else
            {
                // Armor fill
                float normalizedAggressiveThreshold = armorHealth > 0
                    ? (float)armorHealth / maxArmorHealth
                    : 0f;

                _armorFillTween?.Stop();
                _armorFillTween = TweenFX.FadeFloatTo(
                    fromGetter: () => armorFillImage.fillAmount,
                    setter: value => armorFillImage.fillAmount = value,
                    targetValue: normalizedAggressiveThreshold,
                    data: armorFillTweenData,
                    targetObj: this
                );
            }
        }

        private void HandlePhaseStarted(GameState gameState)
        {
            bool isNowBossTurn = gameState.CurrentTurn == ETurnOwner.Boss;
            if (_isBossTurn == isNowBossTurn)
                return;

            _isBossTurn = isNowBossTurn;
            glowVisualFadeTween.Play(!_isBossTurn);
        }

        private void HandleBossAggressive() => lockIconTweenGroup.Play();
    }
}