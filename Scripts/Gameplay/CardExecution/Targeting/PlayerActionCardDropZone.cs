using UnityEngine;
using Gameplay.Cards;
using Gameplay.Player;
using Utility.Logging;
using Systems.Services;
using Gameplay.Cards.Data;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Systems.Tweening.Components.System;

namespace Gameplay.CardExecution.Targeting
{
    /// <summary>
    /// Drop zone for player action cards that targets the player themselves.
    /// </summary>
    public class PlayerActionCardDropZone : MonoBehaviour
    {
        [SerializeField] private TweenGroup fadeTween;
        [SerializeField] private CanvasGroup canvasGroup;

        /// <summary>
        /// Cached reference to the player controller.
        /// </summary>
        public IModifiableBase PlayerController { get; private set; }

        private bool _isActive;

        private void Awake()
        {
            if (ServiceLocator.TryGet(out PlayerController playerController))
                PlayerController = playerController;

            ActionCardController.OnDragStarted += HandleActionCardDragStarted;
            ActionCardController.OnDragEnded += HandleActionCardDragEnded;
        }

        private void OnDestroy()
        {
            ActionCardController.OnDragStarted -= HandleActionCardDragStarted;
            ActionCardController.OnDragEnded -= HandleActionCardDragEnded;

            if (fadeTween == null)
            {
                CustomLogger.LogWarning("Fade tween is null on destroy.", this);
                return;
            }

            fadeTween.OnFinished -= OnFadeOutFinished;
        }

        private void HandleActionCardDragStarted(ActionCardModel actionCardModel)
        {
            if (actionCardModel.ModifierState.ActionCategory is not EActionCategory.Player
                and not EActionCategory.Resource)
                return;

            if (!ServiceLocator.TryGet(out PlayerController playerController))
                return;

            if (!playerController.HasEnoughEnergy(actionCardModel))
                return;

            fadeTween.gameObject.SetActive(true);
            canvasGroup.blocksRaycasts = true;
            fadeTween.Play();
            _isActive = true;
        }

        private void HandleActionCardDragEnded(ActionCardModel actionCardModel)
        {
            if (!_isActive)
                return;

            if (actionCardModel.ModifierState.ActionCategory is not EActionCategory.Player
                and not EActionCategory.Resource)
                return;

            fadeTween.Reverse();
            fadeTween.OnFinished += OnFadeOutFinished;
        }

        private void OnFadeOutFinished()
        {
            fadeTween.OnFinished -= OnFadeOutFinished;
            _isActive = false;
            canvasGroup.blocksRaycasts = false;
            fadeTween.gameObject.SetActive(false);
        }
    }
}