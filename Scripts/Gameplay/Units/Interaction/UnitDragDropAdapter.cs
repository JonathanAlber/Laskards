using System;
using Interaction;
using Managers;
using Systems.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Units.Interaction
{
    /// <summary>
    /// Handles click and drag interactions for a <see cref="UnitController"/>.
    /// Drag behavior is delegated to <see cref="DragResponderUI"/>, which consults <see cref="UnitGate"/>.
    /// This adapter now also selects on drag start, not only on click or drop.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnitDragDropAdapter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnitController unit;

        [Tooltip("Handles drag interactions and moves the RectTransform.")]
        [SerializeField] private DragResponderUI dragResponder;

        [Tooltip("Handles click interactions.")]
        [SerializeField] private ClickResponder clickResponder;

        /// <summary>
        /// Raised when the unit is clicked.
        /// </summary>
        public event Action<UnitController> OnClicked;

        /// <summary>
        /// Raised when the unit drag is started.
        /// </summary>
        public event Action<UnitController> OnDragStarted;

        /// <summary>
        /// Raised while the unit is being dragged.
        /// </summary>
        public event Action<UnitController, PointerEventData> OnDragging;

        /// <summary>
        /// Raised when the unit is dropped (drag released).
        /// </summary>
        public event Action<UnitController, PointerEventData> OnDropped;

        private void Awake()
        {
            InitializeCanvas();

            clickResponder.OnClicked += HandleClicked;
            dragResponder.OnDragStarted += HandleDragStarted;
            dragResponder.OnDragging += HandleDrag;
            dragResponder.OnDragEnded += HandleDragEnded;
        }

        private void OnDestroy()
        {
            clickResponder.OnClicked -= HandleClicked;
            dragResponder.OnDragStarted -= HandleDragStarted;
            dragResponder.OnDragging -= HandleDrag;
            dragResponder.OnDragEnded -= HandleDragEnded;
        }

        private void InitializeCanvas()
        {
            if (ServiceLocator.TryGet(out WorldCanvasProvider worldCanvasProvider))
                dragResponder.Initialize(worldCanvasProvider.WorldCanvas);
        }

        private void HandleClicked(ClickResponder _, PointerEventData __)
        {
            if (!InteractionContext.AllowUnitClicks)
                return;

            OnClicked?.Invoke(unit);
        }

        private void HandleDragStarted(BaseDragResponder _, PointerEventData __)
        {
            if (!InteractionContext.AllowUnitDragging)
                return;

            OnDragStarted?.Invoke(unit);
        }

        private void HandleDrag(BaseDragResponder _, PointerEventData eventData)
        {
            if (!InteractionContext.AllowUnitDragging)
                return;

            OnDragging?.Invoke(unit, eventData);
        }

        private void HandleDragEnded(BaseDragResponder _, PointerEventData eventData)
        {
            if (!InteractionContext.AllowUnitDragging)
                return;

            OnDropped?.Invoke(unit, eventData);
        }
    }
}