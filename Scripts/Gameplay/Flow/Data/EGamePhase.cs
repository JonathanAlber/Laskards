namespace Gameplay.Flow.Data
{
    /// <summary>
    /// Enum representing the different phases of the game.
    /// </summary>
    public enum EGamePhase : byte
    {
        None = 0,

        // Player phases
        PlayerPrePlay = 1,
        PlayerDraw = 2,
        PlayerPlay = 3,
        PlayerMove = 4,

        // Boss phases
        BossPrePlay = 5,
        BossPlay = 6,
        BossAutoMove = 7,
    }
}