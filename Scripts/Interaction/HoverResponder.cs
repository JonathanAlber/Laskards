using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Interaction
{
    /// <summary>
    /// Generic hover interaction that only emits enter/exit events.
    /// </summary>
    public sealed class HoverResponder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// Raised when the pointer enters.
        /// </summary>
        public event Action<HoverResponder, PointerEventData> OnHoverEntered;

        /// <summary>
        /// Raised when the pointer exits.
        /// </summary>
        public event Action<HoverResponder, PointerEventData> OnHoverExited;

        private IInteractionGate _gate;
        private bool _hovering;

        private void Awake() => _gate = GetComponent<IInteractionGate>() ?? new NullInteractionGate();

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_gate.IsTransitioning)
                return;

            _hovering = true;
            OnHoverEntered?.Invoke(this, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_hovering)
                return;

            _hovering = false;

            if (_gate.IsTransitioning)
                return;

            OnHoverExited?.Invoke(this, eventData);
        }
    }
}