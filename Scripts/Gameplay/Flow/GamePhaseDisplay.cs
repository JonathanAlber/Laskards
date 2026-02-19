using Gameplay.Flow.Data;
using TMPro;
using UnityEngine;

namespace Gameplay.Flow
{
    /// <summary>
    /// Displays the current game phase as text in the UI.
    /// </summary>
    public class GamePhaseDisplay : MonoBehaviour
    {
        [Tooltip("A text that displays the current game phase.")]
        [SerializeField] private TMP_Text phaseText;

        private void OnEnable() => GameFlowSystem.OnPhaseStarted += OnPhaseChanged;

        private void OnDisable() => GameFlowSystem.OnPhaseStarted -= OnPhaseChanged;

        private void OnPhaseChanged(GameState state) => phaseText.text = state.CurrentPhase.ToReadableString();
    }
}