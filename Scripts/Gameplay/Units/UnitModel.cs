using System;
using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Model;
using Gameplay.StatLayers.Cards;
using Gameplay.StatLayers.Units;
using Gameplay.Units.Data;
using Utility.Logging;

namespace Gameplay.Units
{
    /// <summary>
    /// Represents the in-game model of a unit, including its stats and state.
    /// </summary>
    public sealed class UnitModel : IStatLayerHost<IUnitStatLayer>
    {
        /// <summary>
        /// Base number of moves a unit has per turn.
        /// </summary>
        public const int BaseMovesPerTurn = 1;

        /// <summary>
        /// Event invoked when the unit's damage changes.
        /// </summary>
        public event Action<int> OnDamageChanged;

        /// <summary>
        /// Event invoked when the unit's remaining move count changes.
        /// </summary>
        public event Action<int> OnRemainingMoveCountChanged;

        /// <summary>
        /// Event invoked when the unit's remaining lifetime changes.
        /// </summary>
        public event Action<int> OnRemainingLifetimeChanged;

        /// <summary>
        /// The Team that owns this unit.
        /// </summary>
        public ETeam Team { get; }

        /// <summary>
        /// Logical type of this unit (A–E).
        /// </summary>
        public EUnitType UnitType { get; }

        /// <summary>
        /// Current hit points of this unit.
        /// </summary>
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// Base damage this unit deals.
        /// </summary>
        public int BaseDamage { get; }

        /// <summary>
        /// The number of turns this unit remains on the field before expiring. -1 means infinite.
        /// </summary>
        public int Lifetime { get; private set; }

        /// <summary>
        /// Amount of energy refunded to the defeating player when this unit is destroyed.
        /// </summary>
        public int Worth { get; }

        /// <summary>
        /// Remaining moves the unit has during the current movement phase.
        /// </summary>
        public int RemainingMoves { get; private set; }

        /// <summary>
        /// Indicates whether this unit is still alive.
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        public IReadOnlyList<IUnitStatLayer> Layers => _layers;
        private readonly List<IUnitStatLayer> _layers = new();

        public UnitModel(UnitCardModel unitCardModel, UnitData unitData, ETeam team)
        {
            Team = team;
            UnitType = unitData.UnitType;
            BaseDamage = unitCardModel.Unit.Damage;
            Worth = unitCardModel.Unit.Worth;
            CurrentHealth = unitCardModel.Unit.BaseHealth;
            Lifetime = unitCardModel.Unit.Lifetime;
            RemainingMoves = 0;
        }

        /// <summary>
        /// Resets the remaining moves to the unit's final move count for the turn.
        /// </summary>
        public void ResetMoves() => SetRemainingMoves(GetFinalMoveCount());

        /// <summary>
        /// Decrements the move count by one.
        /// </summary>
        public void ConsumeMove()
        {
            if (RemainingMoves > 0)
                SetRemainingMoves(RemainingMoves - 1);
        }

        /// <summary>
        /// Resets remaining moves to zero.
        /// </summary>
        public void ClearMoves() => SetRemainingMoves(0);

        /// <summary>
        /// Applies damage to this unit and clamps health between 0 and MaxHealth.
        /// </summary>
        public void ApplyDamage(int amount)
        {
            if (amount <= 0)
                return;

            int newHealth = CurrentHealth - amount;
            if (newHealth < 0)
                newHealth = 0;

            CurrentHealth = newHealth;
        }

        /// <summary>
        /// Reduces the lifetime counter by one. Kills the unit if it reaches zero.
        /// </summary>
        /// <returns>True if the unit should die, otherwise false.</returns>
        public bool DecreaseLifetime()
        {
            if (Lifetime == -1)
                return false;

            Lifetime--;
            OnRemainingLifetimeChanged?.Invoke(Lifetime);
            return Lifetime <= 0;
        }

        /// <summary>
        /// Computes the unit’s damage after all stat layers are applied.
        /// </summary>
        public int GetFinalDamage()
        {
            int result = BaseDamage;
            foreach (IUnitStatLayer layer in _layers)
                result = layer.ModifyDamage(result);

            return result;
        }

        /// <summary>
        /// Increases the unit's health by the specified amount.
        /// </summary>
        public void GainHealth(int amount)
        {
            if (amount <= 0)
            {
                CustomLogger.LogWarning($"Attempted to gain non-positive health amount: {amount}", null);
                return;
            }

            CurrentHealth += amount;
        }

        public void AddLayer(IUnitStatLayer layer)
        {
            _layers.Add(layer);
            RaiseDamageChanged();
        }

        public void RemoveLayer(IUnitStatLayer layer)
        {
            _layers.Remove(layer);
            RaiseDamageChanged();
        }

        public void ClearLayers() => _layers.Clear();

        /// <summary>
        /// Computes the unit’s movement allowance for the turn after stat layers.
        /// </summary>
        private int GetFinalMoveCount()
        {
            int result = BaseMovesPerTurn;
            foreach (IUnitStatLayer layer in _layers)
                result = layer.ModifyMoveCount(result);

            return result;
        }

        private void RaiseDamageChanged()
        {
            int finalDamage = GetFinalDamage();
            OnDamageChanged?.Invoke(finalDamage);
        }

        private void SetRemainingMoves(int moves)
        {
            if (RemainingMoves == moves)
                return;

            RemainingMoves = moves;
            OnRemainingMoveCountChanged?.Invoke(RemainingMoves);
        }
    }
}