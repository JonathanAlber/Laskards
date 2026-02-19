using System.Collections.Generic;
using Gameplay.Flow.Data;
using Systems.Tweening.Components.System;
using TMPro;
using UnityEngine;

namespace Gameplay.Flow
{
    /// <summary>
    /// Displays an animation when the game phase changes.
    /// </summary>
    public class GamePhaseDisplayAnimation : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color playerColor;
        [SerializeField] private Color bossColor;

        [Header("References")]
        [SerializeField] private TweenGroup tweenGroup;
        [SerializeField] private TMP_Text phaseText;

        [Header("Settings")]
        [SerializeField] private List<EGamePhase> phasesToDisplay;

        private void OnEnable() => GameFlowSystem.OnPhaseStarted += OnPhaseChanged;

        private void OnDisable() => GameFlowSystem.OnPhaseStarted -= OnPhaseChanged;

        private void OnPhaseChanged(GameState state)
        {
            if (!phasesToDisplay.Contains(state.CurrentPhase))
                return;

            phaseText.color = state.CurrentTurn == ETurnOwner.Player ? playerColor : bossColor;
            phaseText.text = state.CurrentPhase.ToShortReadableString();
            tweenGroup.Play();
        }
    }
}