namespace Gameplay.Cards.Data
{
    /// <summary>
    /// Enum representing different categories of actions.
    /// </summary>
    public enum EActionCategory : byte
    {
        None = 0,
        Buff = 1,
        Tile = 2,
        Unit = 3,
        Player = 4,
        Resource = 5
    }
}