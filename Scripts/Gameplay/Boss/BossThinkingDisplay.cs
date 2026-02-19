using Gameplay.Movement.AI;
using Systems.Tweening.Components.System;
using UnityEngine;

namespace Gameplay.Boss
{
    /// <summary>
    /// Displays an icon when the boss is thinking.
    /// </summary>
    public class BossThinkingDisplay : MonoBehaviour
    {
        [SerializeField] private RectTransform thinkingIcon;
        [SerializeField] private TweenGroup rotationTweenGroup;
        [SerializeField] private TweenGroup fadeTweenGroup;

        private Vector3 _startingRotation;

        private void Awake()
        {
            BossAutoMoveController.OnBossThinkingStarted += StartThinking;
            BossAutoMoveController.OnBossThinkingEnded += StopThinking;

            BossPlayExecutor.OnBossThinkingStarted += StartThinking;
            BossPlayExecutor.OnBossThinkingEnded += StopThinking;

            _startingRotation = thinkingIcon.eulerAngles;
        }

        private void Start() => ShowThinkingIcon(false);

        private void OnDestroy()
        {
            BossAutoMoveController.OnBossThinkingStarted -= StartThinking;
            BossAutoMoveController.OnBossThinkingEnded -= StopThinking;

            BossPlayExecutor.OnBossThinkingStarted -= StartThinking;
            BossPlayExecutor.OnBossThinkingEnded -= StopThinking;
        }

        private void StartThinking()
        {
            ShowThinkingIcon(true);
            rotationTweenGroup.Play();
        }

        private void StopThinking()
        {
            rotationTweenGroup.Stop();
            ShowThinkingIcon(false);
        }

        private void ShowThinkingIcon(bool show)
        {
            if (show)
            {
                fadeTweenGroup.Play();
            }
            else
            {
                fadeTweenGroup.Reverse();
                fadeTweenGroup.OnFinished += OnFadeFinished;
            }
        }

        private void OnFadeFinished()
        {
            fadeTweenGroup.OnFinished -= OnFadeFinished;
            thinkingIcon.eulerAngles = _startingRotation;
        }
    }
}