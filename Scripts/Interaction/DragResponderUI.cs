using UnityEngine;
using UnityEngine.EventSystems;

namespace Interaction
{
    /// <summary>
    /// Drag responder for UI elements (RectTransforms).
    /// Moves the anchored position within a parent Canvas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class DragResponderUI : BaseDragResponder
    {
        private RectTransform _rect;
        private Canvas _canvas;

        protected override void Awake()
        {
            base.Awake();

            _rect = (RectTransform)transform;
        }

        /// <summary>
        /// Allows late binding of the canvas reference.
        /// </summary>
        public void Initialize(Canvas parentCanvas) => _canvas = parentCanvas;

        protected override void BeginDragInternal(PointerEventData eventData) { }

        protected override void DragInternal(PointerEventData eventData)
        {
            if (_rect.parent is not RectTransform parentRect || _canvas == null)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
                out Vector2 localPoint
            );

            _rect.anchoredPosition = localPoint;
        }

        protected override void EndDragInternal(PointerEventData eventData) { }
    }
}