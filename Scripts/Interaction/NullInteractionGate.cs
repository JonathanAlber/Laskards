namespace Interaction
{
    /// <summary>
    /// A no-op implementation of <see cref="IInteractionGate"/> that never blocks interaction.
    /// </summary>
    public sealed class NullInteractionGate : IInteractionGate
    {
        public bool IsTransitioning => false;

        public bool IsBeingDragged => false;

        public bool AllowDragging => true;

        public void SetDragging(bool _) { }

        public void StartTransition() { }

        public void FinishTransition() { }
    }
}