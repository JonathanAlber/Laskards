using Gameplay.Flow;
using Gameplay.Flow.Data;
using Systems.Tweening.Components.UITweens;
using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// Visual representation of the player in the game.
    /// </summary>
    public class PlayerView : MonoBehaviour
    {
        [Tooltip("Visual tween for glow effect on the player UI representation.")]
        [SerializeField] private FadeTweenInit glowVisualFadeTween;

        private bool _isPlayerTurn;

        private void Awake() => GameFlowSystem.OnPhaseStarted += HandlePhaseStarted;

        private void OnDestroy() => GameFlowSystem.OnPhaseStarted -= HandlePhaseStarted;

        private void HandlePhaseStarted(GameState gameState)
        {
            bool isNowPlayerTurn = gameState.CurrentTurn == ETurnOwner.Player;
            if (_isPlayerTurn == isNowPlayerTurn)
                return;

            _isPlayerTurn = isNowPlayerTurn;
            glowVisualFadeTween.Play(!_isPlayerTurn);
        }
    }
}