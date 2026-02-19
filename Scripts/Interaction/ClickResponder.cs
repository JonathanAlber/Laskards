using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Interaction
{
    /// <summary>
    /// Generic click interaction. Emits an event when clicked.
    /// </summary>
    public sealed class ClickResponder : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Raised when this object is clicked.
        /// </summary>
        public event Action<ClickResponder, PointerEventData> OnClicked;

        private IInteractionGate _gate;

        private void Awake() => _gate = GetComponent<IInteractionGate>() ?? new NullInteractionGate();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_gate.IsTransitioning)
                return;

            OnClicked?.Invoke(this, eventData);
        }
    }
}