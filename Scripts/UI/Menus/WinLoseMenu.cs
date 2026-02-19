using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.GameMetaStats;
using Systems.MenuManaging;
using Systems.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Utility.Collections;

namespace UI.Menus
{
    /// <summary>
    /// Menu that displays win or lose text based on the game outcome.
    /// </summary>
    public class WinLoseMenu : Menu
    {
        [Header("References")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text winLoseText;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color winColor = Color.white;
        [SerializeField] private Color loseColor = Color.white;

        [Header("Texts")]
        [SerializeField] private string[] winTexts;
        [SerializeField] private string[] loseTexts;
        [SerializeField] private string[] winEndings;
        [SerializeField] private string[] loseEndings;

        protected override void Awake()
        {
            base.Awake();

            GameFlowSystem.OnGameOver += HandleGameOver;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameFlowSystem.OnGameOver -= HandleGameOver;
        }

        private void HandleGameOver(GameState state)
        {
            CoroutineRunner.Instance.RunNextFrame(() => SetupMenuContent(state));
        }

        private void SetupMenuContent(GameState state)
        {
            bool playerWon = state.CurrentTurn == ETurnOwner.Player;

            backgroundImage.color = playerWon
                ? winColor
                : loseColor;

            titleText.text = playerWon
                ? winTexts.GetRandomElement()
                : loseTexts.GetRandomElement();

            if (ServiceLocator.TryGet(out GameMetaStatsManager gameMetaStatsManager))
            {
                string endingsText = playerWon
                    ? winEndings.GetRandomElement()
                    : loseEndings.GetRandomElement();

                winLoseText.text = playerWon
                    ? gameMetaStatsManager.BuildWinSummaryText(endingsText)
                    : gameMetaStatsManager.BuildLoseSummaryText(endingsText);
            }
            else
            {
                winLoseText.text = string.Empty;
            }

            Open();
        }
    }
}