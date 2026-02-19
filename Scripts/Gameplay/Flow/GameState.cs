using Gameplay.Flow.Data;

namespace Gameplay.Flow
{
    /// <summary>
    /// Immutable struct representing the current state of the game.
    /// </summary>
    public readonly struct GameState
    {
        /// <summary>
        /// The current active phase of the game.
        /// </summary>
        public EGamePhase CurrentPhase { get; }

        /// <summary>
        /// The current turn owner (e.g., Player or Boss).
        /// </summary>
        public ETurnOwner CurrentTurn { get; }

        /// <summary>
        /// Indicates whether the game has ended.
        /// </summary>
        public bool IsGameOver { get; }

        public GameState(EGamePhase currentPhase, ETurnOwner currentTurn, bool isGameOver)
        {
            CurrentPhase = currentPhase;
            CurrentTurn = currentTurn;
            IsGameOver = isGameOver;
        }

        /// <summary>
        /// Returns a new GameState with the specified phase, preserving other properties.
        /// </summary>
        /// <param name="newPhase">The new game phase.</param>
        /// <returns>A new GameState instance with the updated phase.</returns>
        public GameState WithPhase(EGamePhase newPhase) => new(newPhase, CurrentTurn, IsGameOver);

        /// <summary>
        /// Returns a new GameState with the specified turn owner, preserving other properties.
        /// </summary>
        /// <param name="newTurn">The new turn owner.</param>
        /// <returns>A new GameState instance with the updated turn owner.</returns>
        public GameState WithTurn(ETurnOwner newTurn) => new(CurrentPhase, newTurn, IsGameOver);

        /// <summary>
        /// Returns a new GameState marked as game over, preserving other properties.
        /// </summary>
        /// <returns>A new GameState instance with IsGameOver set to true.</returns>
        public GameState WithGameOver() => new(CurrentPhase, CurrentTurn, true);
    }
}