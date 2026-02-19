namespace Interaction
{
    /// <summary>
    /// Represents all global interaction modes the client can be in.
    /// Controls which input surfaces (cards, tiles, UI) are allowed to react.
    /// </summary>
    public enum EInteractionMode : byte
    {
        /// <summary>
        /// Default input mode. All interactions behave normally.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The player must select a tile. Tile input is enabled;
        /// card interaction is disabled to prevent interference.
        /// </summary>
        TileSelection = 1,

        /// <summary>
        /// The player must select a card. Card clicks are enabled,
        /// but dragging and playing may be disabled depending on rules.
        /// </summary>
        CardSelection = 2,

        /// <summary>
        /// The player must select a unit. Unit clicks are enabled;
        /// card and tile interaction is disabled to prevent interference.
        /// </summary>
        UnitSelection = 3,

        /// <summary>
        /// UI-only mode. Gameplay surfaces do not respond.
        /// Useful for dialogs, confirm popups, etc.
        /// </summary>
        UIModal = 4
    }
}