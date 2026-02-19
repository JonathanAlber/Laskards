using System;
using Gameplay.Boss.Data;
using Systems.Tweening.Components.System;
using Systems.Tweening.Components.UITweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;

namespace UI.Menus.BossSelection
{
    /// <summary>
    /// Component responsible for displaying boss information in the boss selection menu.
    /// </summary>
    public class BossSelectionDisplay : MonoBehaviour
    {
        private const string HpFormat = "{0} HP";

        /// <summary>
        /// Event triggered when the boss is selected.
        /// </summary>
        public event Action<BossSelectionDisplay> OnBossSelected;

        /// <summary>
        /// The boss data associated with this display.
        /// </summary>
        public BossData BossData { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private Button selectButton;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text playstyleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image birdImage;
        [SerializeField] private GameObject difficultyEasyIcon;
        [SerializeField] private GameObject difficultyMediumIcon;
        [SerializeField] private GameObject difficultyHardIcon;
        [SerializeField] private GameObject availableGroup;
        [SerializeField] private GameObject unavailableGroup;
        [SerializeField] private TweenGroup selectedTweenGroup;
        [SerializeField] private TweenGroup fadeTweenGroup;
        [SerializeField] private FadeTweenInit fadeTween;

        private bool _isAvailable;

        private void OnDestroy() => selectButton.onClick.RemoveListener(OnSelectButtonClicked);

        /// <summary>
        /// Initializes the display with the provided boss data.
        /// </summary>
        public void Initialize(BossData bossData)
        {
            BossData = bossData;

            SetAvailability(true);

            nameText.text = BossData.BossName;
            hpText.text = string.Format(HpFormat, BossData.Health.ToString());
            playstyleText.text = BossData.PlaystyleDescription;
            descriptionText.text = BossData.Description;

            birdImage.sprite = BossData.BossImage;

            SetDifficultyElements(BossData.BossDifficulty);

            selectButton.onClick.AddListener(OnSelectButtonClicked);

            fadeTweenGroup.Play();
        }

        /// <summary>
        /// Sets the delay for the fade tween.
        /// </summary>
        public void SetFadeTweenDelay(float delay) => fadeTween.SetDelay(delay);

        /// <summary>
        /// Sets the visibility of the display based on whether the boss is available for selection.
        /// </summary>
        /// <param name="isAvailable"><c>true</c> if the boss data is available; otherwise, <c>false</c>.</param>
        public void SetAvailability(bool isAvailable)
        {
            _isAvailable = isAvailable;
            if (_isAvailable)
            {
                availableGroup.SetActive(true);
                unavailableGroup.SetActive(false);
            }
            else
            {
                availableGroup.SetActive(false);
                unavailableGroup.SetActive(true);
            }
        }

        /// <summary>
        /// Sets the selection state of the display.
        /// </summary>
        /// <param name="isSelected"><c>true</c> if the display is selected; otherwise, <c>false</c>.</param>
        /// <param name="wasSelected"><c>true</c> if the display was previously selected; otherwise, <c>false</c>.</param>
        public void SetSelected(bool isSelected, bool wasSelected)
        {
            if (isSelected)
            {
                selectedTweenGroup.Play();
            }
            else
            {
                if (wasSelected)
                    selectedTweenGroup.Reverse();
                else
                    selectedTweenGroup.SetVisibility(false);
            }
        }

        private void OnSelectButtonClicked()
        {
            if (!_isAvailable)
                return;

            OnBossSelected?.Invoke(this);
        }

        private void SetDifficultyElements(EBossDifficulty bossDataBossDifficulty)
        {
            switch (bossDataBossDifficulty)
            {
                case EBossDifficulty.Easy:
                {
                    difficultyEasyIcon.SetActive(true);
                    difficultyMediumIcon.SetActive(false);
                    difficultyHardIcon.SetActive(false);
                    break;
                }
                case EBossDifficulty.Medium:
                {
                    difficultyEasyIcon.SetActive(true);
                    difficultyMediumIcon.SetActive(true);
                    difficultyHardIcon.SetActive(false);
                    break;
                }
                case EBossDifficulty.Hard:
                {
                    difficultyEasyIcon.SetActive(true);
                    difficultyMediumIcon.SetActive(true);
                    difficultyHardIcon.SetActive(true);
                    break;
                }
                default:
                {
                    CustomLogger.LogWarning($"Unhandled boss difficulty level: {bossDataBossDifficulty}", this);
                    break;
                }
            }
        }
    }
}