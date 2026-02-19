using Gameplay.Flow.Data;

namespace Gameplay.Flow
{
    /// <summary>
    /// Interface for systems that react to specific game phases.
    /// </summary>
    /// <remarks>
    /// It's designed for gameplay systems that participate in the turn-based flow.
    /// Implementers should register themselves with <see cref="GameFlowSystem"/> for their subscribed phases.
    /// </remarks>
    public interface IPhaseSubscriber
    {
        /// <summary>
        /// The list of phases this subscriber participates in.
        /// </summary>
        EGamePhase[] SubscribedPhases { get; }

        /// <summary>
        /// Called automatically by <see cref="GameFlowSystem"/> when the assigned phase begins.
        /// Return an IEnumerator if asynchronous work (e.g. animations, AI, etc.) should delay phase completion.
        /// </summary>
        void OnPhaseStarted(GameState state, GameFlowSystem flow);

        /// <summary>
        /// Called automatically by <see cref="GameFlowSystem"/> when the assigned phase ends.
        /// </summary>
        void OnPhaseEnded(GameState state);
    }
}