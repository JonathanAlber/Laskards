using UnityEngine;
using UnityEngine.EventSystems;

namespace Interaction
{
    /// <summary>
    /// Generic drag responder for 2D world-space objects.
    /// Moves the object's Transform position in world space.
    /// Works with SpriteRenderers and 2D colliders.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class DragResponder2D : BaseDragResponder
    {
        private Vector3 _offset;
        private Camera _worldCamera;

        protected override void Awake()
        {
            base.Awake();

            _worldCamera = Camera.main;
        }

        protected override void BeginDragInternal(PointerEventData eventData)
        {
            Vector3 worldPos = ScreenToWorld(eventData.position);
            _offset = transform.position - worldPos;
        }

        protected override void DragInternal(PointerEventData eventData)
        {
            Vector3 worldPos = ScreenToWorld(eventData.position);
            transform.position = worldPos + _offset;
        }

        protected override void EndDragInternal(PointerEventData eventData) { }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            Vector3 world = _worldCamera.ScreenToWorldPoint
            (
                new Vector3(screenPos.x,
                screenPos.y,
                -_worldCamera.transform.position.z)
            );

            return world;
        }
    }
}