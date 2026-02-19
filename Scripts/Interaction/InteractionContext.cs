using System;

namespace Interaction
{
    /// <summary>
    /// Global input mode manager. Determines which interaction surfaces
    /// (cards, tiles, UI elements) are permitted to react to pointer events.
    /// <br/><br/>
    /// Components that receive user input should query this service before
    /// processing pointer interaction.
    /// </summary>
    public static class InteractionContext
    {
        /// <summary>
        /// Fired whenever the global interaction mode changes.
        /// </summary>
        public static event Action<EInteractionMode> OnModeChanged;

        /// <summary>
        /// The current global interaction mode.
        /// </summary>
        public static EInteractionMode CurrentMode { get; private set; } = EInteractionMode.Normal;

        /// <summary>
        /// Returns <c>true</c>, if card clicks should be allowed in the current mode.
        /// </summary>
        public static bool AllowCardClicks => CurrentMode is EInteractionMode.Normal or EInteractionMode.CardSelection;

        /// <summary>
        /// Returns <c>true</c>, if card hovering should be allowed in the current mode.
        /// </summary>
        public static bool AllowCardHover => CurrentMode is EInteractionMode.Normal or EInteractionMode.CardSelection;

        /// <summary>
        /// Returns <c>true</c>, if card dragging should be allowed in the current mode.
        /// </summary>
        public static bool AllowCardDragging => CurrentMode == EInteractionMode.Normal;

        /// <summary>
        /// Returns <c>true</c>, if tile clicks should be allowed in the current mode.
        /// </summary>
        public static bool AllowTileClicks => CurrentMode is EInteractionMode.Normal or EInteractionMode.TileSelection;

        /// <summary>
        /// Returns <c>true</c>, if unit clicks should be allowed in the current mode.
        /// </summary>
        public static bool AllowUnitClicks => CurrentMode is EInteractionMode.Normal or EInteractionMode.UnitSelection;

        /// <summary>
        /// Returns <c>true</c>, if unit dragging should be allowed in the current mode.
        /// </summary>
        public static bool AllowUnitDragging => CurrentMode is EInteractionMode.Normal or EInteractionMode.UnitSelection;

        /// <summary>
        /// Sets the current interaction mode.
        /// The mode is global and overrides individual component input rules.
        /// </summary>
        /// <param name="mode">New interaction mode.</param>
        public static void SetMode(EInteractionMode mode)
        {
            if (CurrentMode == mode)
                return;

            CurrentMode = mode;
            OnModeChanged?.Invoke(mode);
        }
    }
}