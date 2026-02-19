namespace Gameplay.StatLayers.Units
{
    /// <summary>
    /// A stat layer that increases the unit's move count by a fixed amount.
    /// </summary>
    public class AddMovesLayer : IUnitStatLayer
    {
        private readonly int _amount;

        public AddMovesLayer(int amount) => _amount = amount;

        public int ModifyDamage(int value) => value;

        public int ModifyMaxHealth(int value) => value;

        public int ModifyMoveCount(int value) => value + _amount;
    }
}