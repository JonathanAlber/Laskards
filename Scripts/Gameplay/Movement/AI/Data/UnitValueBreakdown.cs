namespace Gameplay.Movement.AI.Data
{
    /// <summary>
    /// Breakdown of a unit's value into its contributing components.
    /// </summary>
    public struct UnitValueBreakdown
    {
        /// <summary>
        /// ID of the unit this breakdown corresponds to.
        /// </summary>
        public int unitId;

        /// <summary>
        /// Total computed value of the unit.
        /// </summary>
        public float total;

        /// <summary>
        /// Breakdown components contributing to the total value.
        /// </summary>
        public float baseDamagePart;

        /// <summary>
        /// Health contribution to the unit's value.
        /// </summary>
        public float baseHealthPart;

        /// <summary>
        /// Worth contribution to the unit's value.
        /// </summary>
        public float baseWorthPart;

        /// <summary>
        /// Moves contribution to the unit's value.
        /// </summary>
        public float baseMovesPart;

        /// <summary>
        /// Lifetime contribution to the unit's value.
        /// </summary>
        public float lifetimePart;

        /// <summary>
        /// Bonus for infinite lifetime units.
        /// </summary>
        public float infiniteLifetimeBonus;

        /// <summary>
        /// Bonus for unattackable units.
        /// </summary>
        public float unattackableBonus;

        /// <summary>
        /// Temporary damage contribution to the unit's value.
        /// </summary>
        public float tempDamagePart;

        /// <summary>
        /// Temporary moves contribution to the unit's value.
        /// </summary>
        public float tempMovePart;

        public override string ToString()
        {
            return $"Unit {unitId}: total={total:F2} | " +
                   $"base(dmg={baseDamagePart:F2} hp={baseHealthPart:F2} " +
                   $"worth={baseWorthPart:F2} moves={baseMovesPart:F2}) | " +
                   $"temp(dmg={tempDamagePart:F2} moves={tempMovePart:F2}) | " +
                   $"lifetime={lifetimePart:F2} inf={infiniteLifetimeBonus:F2} | " +
                   $"unatk={unattackableBonus:F2}";
        }
    }
}