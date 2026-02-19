namespace Gameplay.StatLayers.Cards
{
    /// <summary>
    /// Temporarily forces a card's energy cost to a specific value
    /// until the layer is removed by runtime logic.
    /// This layer does not define duration; it only defines the cost.
    /// </summary>
    public sealed class SetCostUntilPlayedLayer : ICommonCardStatLayer
    {
        private readonly int _forcedCost;

        /// <summary>
        /// Initializes a new instance of the layer with a specific cost value.
        /// </summary>
        /// <param name="cost">The cost the card should have while this layer is applied.</param>
        public SetCostUntilPlayedLayer(int cost) => _forcedCost = cost;

        /// <summary>
        /// Returns the forced cost regardless of the incoming value.
        /// </summary>
        public int ModifyEnergyCost(int currentCost) => _forcedCost;
    }
}