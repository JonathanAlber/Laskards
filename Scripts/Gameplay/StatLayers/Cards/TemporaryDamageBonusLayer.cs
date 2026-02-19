namespace Gameplay.StatLayers.Cards
{
    /// <summary>
    /// Adds a temporary amount of bonus damage to a unit card.
    /// </summary>
    public sealed class TemporaryDamageBonusLayer : IUnitCardStatLayer
    {
        private readonly int _bonus;

        public TemporaryDamageBonusLayer(int amount) => _bonus = amount;

        public int ModifyDamage(int currentDamage) => currentDamage + _bonus;

        public int ModifyHealth(int currentHealth) => currentHealth;
    }
}