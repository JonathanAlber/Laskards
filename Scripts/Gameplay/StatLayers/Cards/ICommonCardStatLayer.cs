namespace Gameplay.StatLayers.Cards
{
    /// <summary>
    /// Defines modifiers applied to common card values such as energy cost.
    /// </summary>
    public interface ICommonCardStatLayer : IStatLayer
    {
        /// <summary>
        /// Receives the current energy cost and returns an updated value.
        /// Implementations may override, reduce or increase the cost.
        /// </summary>
        int ModifyEnergyCost(int currentCost);
    }
}