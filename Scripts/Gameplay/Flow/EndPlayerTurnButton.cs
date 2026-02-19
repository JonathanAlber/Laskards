using Gameplay.Flow.Data;
using Interaction;
using Systems.Services;
using Systems.Tweening.Components.System;
using TMPro;
using UI.Buttons;
using UnityEngine;

namespace Gameplay.Flow
{
    /// <summary>
    /// Button that allows the player to end their play or move turn.
    /// </summary>
    public class EndPlayerTurnButton : CustomButton, IPhaseSubscriber
    {
        [Header("References")]
        [Tooltip("The parent tween group to activate, when showing the button.")]
        [SerializeField] private TweenGroup contentRoot;
        [SerializeField] private TMP_Text buttonText;

        [Header("Texts")]
        [SerializeField] private string playPhaseText = "End Play Turn";
        [SerializeField] private string movePhaseText = "End Move Turn";

        public EGamePhase[] SubscribedPhases => new[]
        {
            EGamePhase.PlayerPlay,
            EGamePhase.PlayerMove,
        };

        private GameFlowSystem _gameFlowSystem;

        protected override void Awake()
        {
            base.Awake();

            contentRoot.SetVisibility(false);

            if (!ServiceLocator.TryGet(out _gameFlowSystem))
                return;

            foreach (EGamePhase phase in SubscribedPhases)
                _gameFlowSystem.RegisterPhaseSubscriber(phase, this);

            InteractionContext.OnModeChanged += HandleInteractionModeChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            InteractionContext.OnModeChanged -= HandleInteractionModeChanged;
        }

        public void OnPhaseStarted(GameState state, GameFlowSystem flow)
        {
            SetButtonText(GetButtonTextForPhase(state.CurrentPhase));
            contentRoot.SetVisibility(true);
            contentRoot.Play();
        }

        public void OnPhaseEnded(GameState state) => contentRoot.SetVisibility(false);

        protected override void OnClick()
        {
            if (InteractionContext.CurrentMode != EInteractionMode.Normal)
                return;

            _gameFlowSystem.MarkSubscriberDone(this);
            contentRoot.SetVisibility(false);
        }

        private void SetButtonText(string text) => buttonText.text = text;

        private string GetButtonTextForPhase(EGamePhase phase)
        {
            return phase switch
            {
                EGamePhase.PlayerPlay => playPhaseText,
                EGamePhase.PlayerMove => movePhaseText,
                _ => "End Turn"
            };
        }

        private void HandleInteractionModeChanged(EInteractionMode interactionMode)
        {
            if (interactionMode != EInteractionMode.Normal)
                contentRoot.SetVisibility(false);
            else if (GameFlowSystem.CurrentPhase is EGamePhase.PlayerPlay or EGamePhase.PlayerMove)
                contentRoot.SetVisibility(true);
        }
    }
}