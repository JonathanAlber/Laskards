using Gameplay.Flow.Data;
using TMPro;
using UnityEngine;

namespace Gameplay.Flow
{
    /// <summary>
    /// Displays the current round number in the UI.
    /// </summary>
    public class RoundCounterDisplay : MonoBehaviour
    {
        [Tooltip("A format string for displaying the round number. The round number will be inserted at '{0}'.")]
        [SerializeField] private string textFormat = "Round {0}";

        [Tooltip("A text that displays the current round number.")]
        [SerializeField] private TMP_Text roundText;

        private int _roundsPlayed;

        private void OnEnable() => GameFlowSystem.OnPhaseStarted += HandlePhaseStarted;

        private void OnDisable() => GameFlowSystem.OnPhaseStarted -= HandlePhaseStarted;

        private void HandlePhaseStarted(GameState gameState)
        {
            if (gameState.CurrentPhase is not EGamePhase.BossPrePlay)
                return;

            _roundsPlayed++;
            roundText.text = string.Format(textFormat, _roundsPlayed);
        }
    }
}