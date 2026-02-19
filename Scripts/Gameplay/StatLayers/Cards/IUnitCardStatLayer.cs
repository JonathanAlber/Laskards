namespace Gameplay.StatLayers.Cards
{
    /// <summary>
    /// Defines modifiers applied to unit-specific values such as damage and health.
    /// </summary>
    public interface IUnitCardStatLayer : IStatLayer
    {
        /// <summary>
        /// Receives the current damage value and returns an updated value.
        /// </summary>
        int ModifyDamage(int currentDamage);

        /// <summary>
        /// Receives the current health value and returns an updated value.
        /// </summary>
        int ModifyHealth(int currentHealth);
    }
}