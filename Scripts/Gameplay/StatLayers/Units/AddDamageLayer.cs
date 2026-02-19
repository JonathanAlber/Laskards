namespace Gameplay.StatLayers.Units
{
    /// <summary>
    /// A stat layer that increases the unit's damage by a fixed amount.
    /// </summary>
    public sealed class AddDamageLayer : IUnitStatLayer
    {
        private readonly int _amount;

        public AddDamageLayer(int amount) => _amount = amount;

        public int ModifyDamage(int value) => value + _amount;

        public int ModifyMaxHealth(int value) => value;

        public int ModifyMoveCount(int value) => value;
    }
}