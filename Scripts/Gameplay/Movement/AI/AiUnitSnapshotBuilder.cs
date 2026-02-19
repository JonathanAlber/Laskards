using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Units;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Builder used to construct modified <see cref="AiUnitSnapshot"/> instances.
    /// This simplifies creating updated immutable snapshots without manually
    /// re-specifying all constructor parameters.
    /// </summary>
    public sealed class AiUnitSnapshotBuilder
    {
        private readonly int _id;
        private readonly ETeam _team;
        private readonly EUnitType _unitType;

        private readonly int _damage;
        private readonly int _worth;

        private int _row;
        private int _column;
        private int _currentHealth;
        private int _lifetime;
        private int _movesLeft;

        private IReadOnlyList<AiUnitEffectSnapshot> _effectSnapshots;

        /// <summary>
        /// Creates a new builder derived from an existing <see cref="AiUnitSnapshot"/>.
        /// </summary>
        /// <param name="source">The snapshot used as the base template.</param>
        public AiUnitSnapshotBuilder(AiUnitSnapshot source)
        {
            _id = source.Id;
            _team = source.Team;
            _unitType = source.UnitType;

            _row = source.Row;
            _column = source.Column;
            _currentHealth = source.CurrentHealth;
            _damage = source.Damage;
            _lifetime = source.Lifetime;
            _worth = source.Worth;

            _movesLeft = source.MovesLeft;
            _effectSnapshots = source.Effects;
        }

        /// <summary>
        /// Sets the board position (row, column) for the unit.
        /// </summary>
        public AiUnitSnapshotBuilder SetPosition(int newRow, int newColumn)
        {
            _row = newRow;
            _column = newColumn;
            return this;
        }

        /// <summary>
        /// Sets the lifetime value for the unit.
        /// </summary>
        public AiUnitSnapshotBuilder SetLifetime(int newLifetime)
        {
            _lifetime = newLifetime;
            return this;
        }

        /// <summary>
        /// Sets the current health of the unit.
        /// </summary>
        public AiUnitSnapshotBuilder SetHealth(int newHealth)
        {
            _currentHealth = newHealth;
            return this;
        }

        /// <summary>
        /// Sets the number of moves left for the current phase.
        /// </summary>
        public AiUnitSnapshotBuilder SetMovesLeft(int newMovesLeft)
        {
            _movesLeft = newMovesLeft;
            return this;
        }

        /// <summary>
        /// Replaces the unit's effect list and updates both maximum
        /// and remaining moves for the phase.
        /// </summary>
        /// <param name="newEffects">New effect snapshots.</param>
        public AiUnitSnapshotBuilder SetEffects(IReadOnlyList<AiUnitEffectSnapshot> newEffects)
        {
            _effectSnapshots = newEffects;
            return this;
        }

        /// <summary>
        /// Builds a fully constructed <see cref="AiUnitSnapshot"/> from this builder.
        /// </summary>
        public AiUnitSnapshot Build()
        {
            return new AiUnitSnapshot(
                _id,
                _team,
                _unitType,
                _row,
                _column,
                _currentHealth,
                _damage,
                _lifetime,
                _worth,
                _movesLeft,
                _effectSnapshots
            );
        }
    }
}