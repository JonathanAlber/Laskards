namespace Gameplay.Decks
{
    /// <summary>
    /// Implemented by owners that want to be notified when discard finished returning cards.
    /// </summary>
    public interface IDiscardRecycleReceiver
    {
        /// <summary>
        /// Called once all cards destined for this owner have finished animating from discard back to the owner.
        /// </summary>
        void OnDiscardRecycleCompleted();
    }
}