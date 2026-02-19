using Gameplay.CardExecution;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Interaction;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Units.Interaction
{
    /// <summary>
    /// Interaction gate for a <see cref="UnitController"/>, controlling when dragging is permitted.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnitGate : MonoBehaviour, IInteractionGate
    {
        public bool IsTransitioning { get; private set; }
        public bool IsBeingDragged { get; private set; }

        /// <summary>
        /// Dragging is allowed only if:
        /// <list type="bullet">
        /// <item><description>The current game phase is <see cref="EGamePhase.PlayerMove"/>.</description></item>
        /// <item><description>The unit belongs to <see cref="ETeam.Player"/>.</description></item>
        /// <item><description>No transitions are active.</description></item>
        /// </list>
        /// </summary>
        public bool AllowDragging => !IsTransitioning && _unit.Model.Team == ETeam.Player &&
                                     _currentPhase == EGamePhase.PlayerMove;

        private UnitController _unit;
        private EGamePhase _currentPhase = EGamePhase.None;

        private void Awake()
        {
            _unit = GetComponent<UnitController>();
            if (_unit == null)
                CustomLogger.LogError($"{nameof(UnitGate)} requires a {nameof(UnitController)} on the same GameObject.", this);

            GameFlowSystem.OnPhaseStarted += HandlePhaseStarted;
            GameFlowSystem.OnPhaseEnded += HandlePhaseEnded;
        }

        private void OnDestroy()
        {
            GameFlowSystem.OnPhaseStarted -= HandlePhaseStarted;
            GameFlowSystem.OnPhaseEnded -= HandlePhaseEnded;
        }

        public void SetDragging(bool isDragging) => IsBeingDragged = isDragging;

        public void StartTransition() => IsTransitioning = true;

        public void FinishTransition() => IsTransitioning = false;

        private void HandlePhaseStarted(GameState state)
        {
            _currentPhase = state.CurrentPhase;
        }

        private void HandlePhaseEnded(GameState state)
        {
            if (state.CurrentPhase == _currentPhase)
                _currentPhase = EGamePhase.None;
        }
    }
}