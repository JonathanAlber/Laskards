namespace Interaction
{
    /// <summary>
    /// Minimal interaction gate that components can consult
    /// to determine whether input should be ignored.
    /// </summary>
    public interface IInteractionGate
    {
        /// <summary>
        /// <c>true</c> while an animation/transition is blocking input.
        /// </summary>
        bool IsTransitioning { get; }

        /// <summary>
        /// <c>true</c> while this object is currently being dragged.
        /// </summary>
        bool IsBeingDragged { get; }

        /// <summary>
        /// Returns whether dragging is allowed for this object.
        /// </summary>
        bool AllowDragging { get; }

        /// <summary>
        /// Sets dragging flag for this object.
        /// </summary>
        /// <param name="isDragging">Whether dragging is active.</param>
        void SetDragging(bool isDragging);

        /// <summary>
        /// Marks transitions as started (blocks input).
        /// </summary>
        void StartTransition();

        /// <summary>
        /// Marks transitions as finished (unblocks input).
        /// </summary>
        void FinishTransition();
    }
}