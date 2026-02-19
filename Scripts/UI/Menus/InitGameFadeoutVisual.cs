using Gameplay.Flow.Gate;
using Systems.Tweening.Components.System;
using UnityEngine;

namespace UI.Menus
{
    /// <summary>
    /// Manages the fade-out visual effect when the game initialization is complete.
    /// </summary>
    public class InitGameFadeoutVisual : MonoBehaviour
    {
        [SerializeField] private TweenGroup fadeoutTweenGroup;

        private void Awake() => InitBarrier.OnBecameReady += HandleInitGateReady;

        private void OnDestroy() => InitBarrier.OnBecameReady -= HandleInitGateReady;

        private void HandleInitGateReady() => fadeoutTweenGroup.Reverse();
    }
}