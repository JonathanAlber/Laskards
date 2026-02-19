using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Interaction
{
    /// <summary>
    /// Base class for pointer drag responders.
    /// Handles event dispatch and drag gating; derived types perform actual movement.
    /// </summary>
    public abstract class BaseDragResponder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float DefaultDragThreshold = 5f;

        /// <summary>
        /// Raised when a drag begins.
        /// </summary>
        public event Action<BaseDragResponder, PointerEventData> OnDragStarted;

        /// <summary>
        /// Raised while dragging.
        /// </summary>
        public event Action<BaseDragResponder, PointerEventData> OnDragging;

        /// <summary>
        /// Raised when the drag ends.
        /// </summary>
        public event Action<BaseDragResponder, PointerEventData> OnDragEnded;

        private IInteractionGate _gate;
        private bool _isDraggable = true;
        private bool _dragStarted;
        private Vector2 _dragStartPos;
        private float _dragThreshold;

        protected virtual void Awake()
        {
            _gate = GetComponent<IInteractionGate>() ?? new NullInteractionGate();
            _dragThreshold = EventSystem.current != null
                ? EventSystem.current.pixelDragThreshold
                : DefaultDragThreshold;
        }

        public void SetDraggable(bool draggable) => _isDraggable = draggable;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDraggable || _gate.IsTransitioning || !_gate.AllowDragging)
                return;

            _dragStarted = false;
            _dragStartPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDraggable || _gate.IsTransitioning || !_gate.AllowDragging)
                return;

            if (!_dragStarted)
            {
                if (Vector2.Distance(eventData.position, _dragStartPos) < _dragThreshold)
                    return;

                _dragStarted = true;
                _gate.SetDragging(true);
                OnDragStarted?.Invoke(this, eventData);
                BeginDragInternal(eventData);
            }

            DragInternal(eventData);
            OnDragging?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_gate.IsBeingDragged)
            {
                _dragStarted = false;
                return;
            }

            _gate.SetDragging(false);
            EndDragInternal(eventData);
            OnDragEnded?.Invoke(this, eventData);
            _dragStarted = false;
        }

        protected abstract void BeginDragInternal(PointerEventData eventData);
        protected abstract void DragInternal(PointerEventData eventData);
        protected abstract void EndDragInternal(PointerEventData eventData);
    }
}