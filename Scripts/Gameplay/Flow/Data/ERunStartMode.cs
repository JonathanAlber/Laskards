namespace Gameplay.Flow.Data
{
    /// <summary>
    /// Defines the mode for starting a new run in the game.
    /// </summary>
    public enum ERunStartMode : byte
    {
        NewRun = 0, // clear boss + deck
        Retry = 1, // keep boss + deck
        ChangeBoss = 2, // clear boss only
        ChangeDeck = 3 // clear deck only
    }
}