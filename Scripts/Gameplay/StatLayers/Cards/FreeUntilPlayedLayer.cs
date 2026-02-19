namespace Gameplay.StatLayers.Cards
{
    /// <summary>
    /// Temporarily overrides a card's energy cost to zero until the card is played.
    /// </summary>
    public sealed class FreeUntilPlayedLayer : ICommonCardStatLayer
    {
        public int ModifyEnergyCost(int currentCost) => 0;
    }
}