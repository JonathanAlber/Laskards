namespace Gameplay.StatLayers.Units
{
    /// <summary>
    /// Represents a pure, stat-transforming layer applied to a unit.
    /// Layers do not store references to runtime objects and contain 
    /// no timing or state logic—only mathematical modification.
    /// </summary>
    public interface IUnitStatLayer : IStatLayer
    {
        /// <summary>
        /// Modifies the unit's final damage value.
        /// </summary>
        /// <param name="value">The current damage value before this layer is applied.</param>
        /// <returns>The modified damage value.</returns>
        int ModifyDamage(int value);

        /// <summary>
        /// Modifies the unit's final max health.
        /// </summary>
        /// <param name="value">The current max health before this layer.</param>
        /// <returns>The modified max health.</returns>
        int ModifyMaxHealth(int value);

        /// <summary>
        /// Modifies the final move count the unit receives at the start of its turn.
        /// </summary>
        /// <param name="value">The current move count before this layer.</param>
        /// <returns>The modified move count.</returns>
        int ModifyMoveCount(int value);
    }
}