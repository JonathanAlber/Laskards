using System.Collections.Generic;
using System.Linq;
using Gameplay.CardExecution;
using Gameplay.Units;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Immutable snapshot of a single unit relevant for AI evaluation and search.
    /// </summary>
    public sealed class AiUnitSnapshot
    {
        /// <summary>
        /// Numeric identifier used to map this snapshot back to a live UnitController.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Team owning this unit.
        /// </summary>
        public readonly ETeam Team;

        /// <summary>
        /// Logical unit type used for movement (A–E).
        /// </summary>
        public readonly EUnitType UnitType;

        /// <summary>
        /// Current row of this unit on the board.
        /// </summary>
        public readonly int Row;

        /// <summary>
        /// Current column of this unit on the board.
        /// </summary>
        public readonly int Column;

        /// <summary>
        /// Current hit points.
        /// </summary>
        public readonly int CurrentHealth;

        /// <summary>
        /// Base damage dealt by this unit.
        /// </summary>
        public readonly int Damage;

        /// <summary>
        /// Remaining lifetime. -1 means infinite, 0 means should already be dead.
        /// </summary>
        public readonly int Lifetime;

        /// <summary>
        /// Worth of this unit when defeated.
        /// </summary>
        public readonly int Worth;

        /// <summary>
        /// Remaining moves this unit can perform in the current phase.
        /// </summary>
        public readonly int MovesLeft;

        /// <summary>
        /// Snapshot of all effects that still matter for future turns.
        /// </summary>
        public readonly IReadOnlyList<AiUnitEffectSnapshot> Effects;

        /// <summary>
        /// Indicates whether this unit is considered alive.
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// Indicates whether this unit can currently be attacked.
        /// </summary>
        public bool CanBeAttacked => Effects.Count == 0 || Effects.All(e => e.CanBeAttacked);

        public AiUnitSnapshot(int id, ETeam team, EUnitType unitType, int row, int column, int currentHealth,
            int damage, int lifetime, int worth, int movesLeft, IReadOnlyList<AiUnitEffectSnapshot> effects)
        {
            Id = id;
            Team = team;
            UnitType = unitType;
            Row = row;
            Column = column;
            CurrentHealth = currentHealth;
            Damage = damage;
            Lifetime = lifetime;
            Worth = worth;
            MovesLeft = movesLeft;
            Effects = effects;
        }

        /// <summary>
        /// Creates a builder pre-populated with this snapshot's data.
        /// </summary>
        public AiUnitSnapshotBuilder ToBuilder() => new(this);

        /// <summary>
        /// Damage after applying all active unit stat layers from effects.
        /// </summary>
        public int GetEffectiveDamage()
        {
            int result = Damage;
            foreach (AiUnitEffectSnapshot effect in Effects)
            {
                if (effect.StatLayer != null)
                    result = effect.StatLayer.ModifyDamage(result);
            }

            return result;
        }

        /// <summary>
        /// Move allowance for this unit at the start of its turn based on current active stat layers.
        /// </summary>
        public int GetEffectiveMoveCount()
        {
            int result = UnitModel.BaseMovesPerTurn;
            foreach (AiUnitEffectSnapshot effect in Effects)
            {
                if (effect.StatLayer != null)
                    result = effect.StatLayer.ModifyMoveCount(result);
            }

            return result;
        }
    }
}