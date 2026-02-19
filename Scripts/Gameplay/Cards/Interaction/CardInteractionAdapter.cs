using System;
using Gameplay.Decks.Zones;
using Interaction;
using Systems.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Logging;

namespace Gameplay.Cards.Interaction
{
    /// <summary>
    /// Wires generic interaction (click/drag/hover) to card gameplay and zone rules.
    /// Keeps all card-specific behavior here, leaving base components reusable.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CardController))]
    public sealed class CardInteractionAdapter : MonoBehaviour
    {
        /// <summary>
        /// Raised when this card is clicked.
        /// </summary>
        public event Action<CardController> OnCardClicked;

        /// <summary>
        /// Raised when dragging of this card starts.
        /// </summary>
        public event Action<CardController> OnDragStarted;

        /// <summary>
        /// Raised while this card is being dragged.
        /// </summary>
        public event Action<CardController> OnDragging;

        /// <summary>
        /// Raised when dragging of this card ends.
        /// </summary>
        public event Action<CardController> OnDragEnded;

        [Header("References")]
        [Tooltip("Handles click interactions.")]
        [SerializeField] private ClickResponder click;

        [Tooltip("Handles drag interactions.")]
        [SerializeField] private DragResponderUI drag;

        [Tooltip("Handles hover interactions.")]
        [SerializeField] private HoverResponder hover;

        private CardController _card;
        private ICardZone _zone;

        private void Awake()
        {
            _card = GetComponent<CardController>();

            click.OnClicked += HandleClicked;
            drag.OnDragStarted += HandleDragStarted;
            drag.OnDragging += HandleDragging;
            drag.OnDragEnded += HandleDragEnded;
            hover.OnHoverEntered += HandleHoverEntered;
            hover.OnHoverExited += HandleHoverExited;
        }

        private void OnDestroy()
        {
            click.OnClicked -= HandleClicked;
            drag.OnDragStarted -= HandleDragStarted;
            drag.OnDragging -= HandleDragging;
            drag.OnDragEnded -= HandleDragEnded;
            hover.OnHoverEntered -= HandleHoverEntered;
            hover.OnHoverExited -= HandleHoverExited;
        }

        /// <summary>
        /// Declares which zone currently owns this card.
        /// </summary>
        public void SetZone(ICardZone zone) => _zone = zone;

        /// <summary>
        /// Enables or disables drag ability.
        /// </summary>
        public void SetDraggable(bool isDraggable) => drag.SetDraggable(isDraggable);

        /// <summary>
        /// For UI cards, binds the canvas for pointer coordinate conversion.
        /// </summary>
        public void InitializeCanvas(Canvas canvas) => drag.Initialize(canvas);

        private void HandleClicked(ClickResponder _, PointerEventData __)
        {
            if (!InteractionContext.AllowCardClicks)
                return;

            OnCardClicked?.Invoke(_card);
        }

        private void HandleDragStarted(BaseDragResponder _, PointerEventData __)
        {
            if (!InteractionContext.AllowCardDragging)
                return;

            _zone.OnDragStarted(_card);

            OnDragStarted?.Invoke(_card);
        }

        private void HandleDragging(BaseDragResponder _, PointerEventData __)
        {
            if (!InteractionContext.AllowCardDragging)
                return;

            OnDragging?.Invoke(_card);
        }

        private void HandleDragEnded(BaseDragResponder dragResponder, PointerEventData eventData)
        {
            if (!InteractionContext.AllowCardDragging)
            {
                _zone?.ReturnCard(_card);
                return;
            }

            OnDragEnded?.Invoke(_card);

            if (_zone.TryAcceptPlay(_card, out string failReason))
                return;

            if (!string.IsNullOrEmpty(failReason))
                CustomLogger.LogWarning("Card play failed: " + failReason, this);

            _zone?.ReturnCard(_card);
        }

        private void HandleHoverEntered(HoverResponder _, PointerEventData __)
        {
            if (!InteractionContext.AllowCardHover)
                return;

            _zone?.OnHoverEnter(_card);
        }

        private void HandleHoverExited(HoverResponder _, PointerEventData __)
        {
            if (!InteractionContext.AllowCardHover)
                return;

            _zone?.OnHoverExit(_card);
        }
    }
}