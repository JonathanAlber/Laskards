using Gameplay.Boss.Data;
using Gameplay.Flow;
using Systems.MenuManaging;
using Systems.Services;
using Systems.Tweening.Components.System;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;

namespace UI.Menus.BossSelection
{
    /// <summary>
    /// Menu allowing the player to select a boss to face.
    /// </summary>
    public class BossSelectionMenu : Menu
    {
        [Header("Boss Selection Menu")]
        [SerializeField] private BossDataCollection bossDataCollection;
        [SerializeField] private BossSelectionDisplay[] bossSelectionDisplays;

        [Header("UI Elements")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private TweenGroup confirmButtonTweenGroup;

        [Header("Tween Settings")]
        [SerializeField] private float baseDisplayFadeDuration = 0.2f;

        private BossSelectionDisplay _selectedBossDisplay;

        protected override void Start()
        {
            base.Start();

            confirmButtonTweenGroup.SetVisibility(false);

            PopulateDisplays();

            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            confirmButton.onClick.RemoveAllListeners();

            foreach (BossSelectionDisplay display in bossSelectionDisplays)
                display.OnBossSelected -= OnBossSelected;
        }

        private void PopulateDisplays()
        {
            if (bossDataCollection.BossData.Count != bossSelectionDisplays.Length)
            {
                CustomLogger.LogWarning($"Boss data count ({bossDataCollection.BossData.Count}) does not match the " +
                                        $"number of display elements ({bossSelectionDisplays.Length}). Some displays" +
                                        " may be inactive.", this);
            }

            for (int i = 0; i < bossSelectionDisplays.Length; i++)
            {
                BossSelectionDisplay bossSelectionDisplay = bossSelectionDisplays[i];
                if (i < bossDataCollection.BossData.Count)
                {
                    BossData data = bossDataCollection.BossData[i];
                    bossSelectionDisplay.SetFadeTweenDelay((i + 1) * baseDisplayFadeDuration);
                    bossSelectionDisplay.Initialize(data);
                    bossSelectionDisplay.OnBossSelected += OnBossSelected;
                }
                else
                {
                    bossSelectionDisplay.SetAvailability(false);
                }
            }
        }

        private void OnBossSelected(BossSelectionDisplay bossSelectionDisplay)
        {
            if (_selectedBossDisplay == bossSelectionDisplay)
                return;

            if (_selectedBossDisplay == null)
                confirmButtonTweenGroup.Play();

            foreach (BossSelectionDisplay display in bossSelectionDisplays)
                display.SetSelected(display == bossSelectionDisplay, display == _selectedBossDisplay);

            _selectedBossDisplay = bossSelectionDisplay;
        }

        private void OnConfirmButtonClicked()
        {
            if (_selectedBossDisplay == null)
            {
                CustomLogger.LogWarning("Confirm button clicked but no boss has been selected.", this);
                return;
            }

            if (!ServiceLocator.TryGet(out GameContextService gameContextService))
                return;

            BossData boss = _selectedBossDisplay.BossData;

            gameContextService.ChooseBoss(boss);
        }
    }
}