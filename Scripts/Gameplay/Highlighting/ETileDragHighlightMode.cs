namespace Gameplay.Highlighting
{
    /// <summary>
    /// Modes for highlighting tiles when dragging tile-targeting action cards.
    /// </summary>
    public enum ETileDragHighlightMode : byte
    {
        None = 0,
        HighlightOccupied = 1,
        HighlightFree = 2,
        HighlightNonUnits = 3,
        HighlightAll = 4
    }
}