using Gameplay.Flow;
using Gameplay.Flow.Data;
using UnityEngine;

namespace Gameplay.Hand
{
    /// <summary>
    /// Manages the player's hand UI, enabling or disabling card interactions based on the game phase.
    /// </summary>
    public class HandUI : MonoBehaviour
    {
        private void OnEnable() => GameFlowSystem.OnPhaseStarted += OnPhaseChanged;

        private void OnDisable() => GameFlowSystem.OnPhaseStarted -= OnPhaseChanged;

        private static void OnPhaseChanged(GameState state)
        {
            EnableCardInteraction(state.CurrentPhase == EGamePhase.PlayerPlay);
        }

        private static void EnableCardInteraction(bool enable)
        {
            /* // Exemplary implementation using a CanvasGroup
             if (canvasGroup == null)
                return;

            canvasGroup.interactable = enable;
            canvasGroup.blocksRaycasts = enable;
            canvasGroup.alpha = enable ? 1f : 0.6f;*/
        }
    }
}