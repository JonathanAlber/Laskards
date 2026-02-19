using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Board;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier;
using Gameplay.CardExecution;
using Gameplay.Cards.Effects.Display;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Units.Data;
using Gameplay.Units.Interaction;
using Systems.ObjectPooling.Gameplay;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Units
{
    /// <summary>
    /// Controller for a unit instance.
    /// Bridges the model, view and movement strategy and exposes a simple API to the rest of the game.
    /// </summary>
    public class UnitController : MonoBehaviour, IModifiable<UnitEffect>
    {
        /// <summary>
        /// Event invoked when the unit dies.
        /// </summary>
        public event Action<UnitController> OnUnitDied;

        /// <summary>
        /// Event invoked when the unit takes damage. Parameters are the damaged unit and the attacker unit.
        /// </summary>
        public event Action<UnitController, UnitController, int, EDamageKind> OnDamaged;

        [field: Header("References")]
        [SerializeField] private UnitEffectDisplay[] effectDisplays;

        /// <summary>
        /// The drag and drop adapter for the unit.
        /// </summary>
        [field: SerializeField] public UnitDragDropAdapter DragDropAdapter { get; private set; }

        /// <summary>
        /// The view component for the unit.
        /// </summary>
        [field: SerializeField] public UnitView View { get; private set; }

        [Header("Effect Settings")]
        [Tooltip("Maximum number of active effects that can be applied to this unit.")]
        [SerializeField] private int maxActiveEffects = 2;

        /// <summary>
        /// The tile the unit is currently attached to.
        /// </summary>
        public Tile CurrentTile { get; private set; }

        /// <summary>
        /// The data model for the unit.
        /// </summary>
        public UnitModel Model { get; private set; }

        /// <summary>
        /// The team affiliation of the unit.
        /// </summary>
        public ETeam Team => Model?.Team ?? ETeam.Player;

        public List<UnitEffect> ActiveEffects { get; } = new();

        private void Awake()
        {
            foreach (UnitEffectDisplay effectDisplay in effectDisplays)
                effectDisplay.SetComponentsEnabled(false);
        }

        private void OnEnable()
        {
            GameFlowSystem.OnPhaseStarted += HandlePhaseStarted;
            GameFlowSystem.OnPhaseEnded += HandlePhaseEnded;
        }

        private void OnDisable()
        {
            GameFlowSystem.OnPhaseStarted -= HandlePhaseStarted;
            GameFlowSystem.OnPhaseEnded -= HandlePhaseEnded;
        }

        private void OnDestroy()
        {
            foreach (UnitEffect effect in ActiveEffects)
                effect.Revert();
        }

        /// <summary>
        /// Initializes the unit controller with the specified model, tile and unit data.
        /// </summary>
        /// <param name="unitModel">The data model for the unit.</param>
        /// <param name="tile">The tile to attach the unit to.</param>
        public void Initialize(UnitModel unitModel, Tile tile)
        {
            Model = unitModel;
            View.Initialize(unitModel);
            AttachToTile(tile);
        }

        /// <summary>
        /// Attaches the unit to the specified tile.
        /// </summary>
        /// <param name="tile">The tile to attach the unit to.</param>
        /// <param name="snapToPosition">
        /// If set to <c>true</c>, the unit's position will be snapped to the tile's position.
        /// </param>
        public void AttachToTile(Tile tile, bool snapToPosition = true)
        {
            if (tile == null)
            {
                CustomLogger.LogWarning("Cannot attach unit to a null tile.", this);
                return;
            }

            CurrentTile = tile;

            if (!snapToPosition)
                return;

            Transform unitTransform = transform;
            unitTransform.position = tile.transform.position;
        }

        /// <summary>
        /// Checks if the unit can be attacked, considering its active effects.
        /// </summary>
        /// <returns><c>true</c> if the unit can be attacked; otherwise, <c>false</c>.</returns>
        public bool CanBeAttacked()
        {
            foreach (UnitEffect unitEffect in ActiveEffects)
            {
                if (!unitEffect.CanBeAttacked)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Applies damage to the unit and checks for death.
        /// </summary>
        /// <param name="amount">The amount of damage to apply.</param>
        /// <param name = "attacker">The unit that is attacking.</param>
        /// <param name = "damageKind">If the damage is normal or reflected.</param>
        public void ApplyDamage(int amount, UnitController attacker, EDamageKind damageKind = EDamageKind.Normal)
        {
            Model.ApplyDamage(amount);
            OnDamaged?.Invoke(this, attacker, amount, damageKind);

            if (!Model.IsAlive)
                Die();
        }

        /// <summary>
        /// Handles the unit's death.
        /// </summary>
        public void Die()
        {
            OnUnitDied?.Invoke(this);
            ClearEffects();
            UnitPool.Instance.Release(this);
        }

        public void AddEffect(UnitEffect effect)
        {
            if (effect == null)
            {
                CustomLogger.LogWarning("Cannot add a null effect to the unit.", this);
                return;
            }

            if (HasMaxEffectsReached())
            {
                CustomLogger.LogWarning("Cannot add effect: maximum number of active effects reached.", this);
                return;
            }

            if (ActiveEffects.Contains(effect))
            {
                CustomLogger.LogWarning("Cannot add effect: effect is already active on the unit.", this);
                return;
            }

            ActiveEffects.Add(effect);
            effect.Apply();

            foreach (UnitEffectDisplay display in effectDisplays)
            {
                if (display.HasEffect)
                    continue;

                display.SetEffect(effect);
                break;
            }
        }

        public void RemoveEffect(UnitEffect effect)
        {
            if (effect == null)
            {
                CustomLogger.LogWarning("Cannot remove a null effect from the unit.", this);
                return;
            }

            if (!ActiveEffects.Contains(effect))
            {
                CustomLogger.LogWarning("Cannot remove effect: effect not found among active effects.", this);
                return;
            }

            ActiveEffects.Remove(effect);
            effect.Revert();

            foreach (UnitEffectDisplay display in effectDisplays)
            {
                if (display.EffectData != effect.EffectData)
                    continue;

                display.ClearEffect();
                break;
            }
        }

        public void ClearEffects()
        {
            foreach (UnitEffect effect in ActiveEffects.ToList())
                RemoveEffect(effect);
        }

        public void AddEffect(IEffect effect)
        {
            if (effect is not UnitEffect unitEffect)
                return;

            AddEffect(unitEffect);
        }

        public void RemoveEffect(IEffect effect)
        {
            if (effect is not UnitEffect unitEffect)
                return;

            RemoveEffect(unitEffect);
        }

        public void TickEffects()
        {
            List<UnitEffect> expired = new();

            foreach (UnitEffect effect in ActiveEffects)
            {
                effect.Tick();

                if (effect.IsExpired)
                    expired.Add(effect);
            }

            foreach (UnitEffect effect in expired)
            {
                effect.Revert();
                RemoveEffect(effect);
            }
        }

        /// <summary>
        /// Highlights or unhighlights all active effects on the unit.
        /// </summary>
        public void SetHighlightEffect(EHighlightMode mode)
        {
            foreach (UnitEffectDisplay display in effectDisplays)
            {
                if (display.HasEffect)
                    continue;

                display.SetHighlight(mode);
                return;
            }
        }

        /// <summary>
        /// Checks if the maximum number of active effects has been reached.
        /// </summary>
        /// <returns><c>true</c> if the maximum number of active effects is reached; otherwise, <c>false</c>.</returns>
        public bool HasMaxEffectsReached() => ActiveEffects.Count >= maxActiveEffects;

        public bool ValidatePlayer(ETeam team) => Team == team;

        private void HandlePhaseStarted(GameState state)
        {
            if (state.CurrentPhase != EGamePhase.PlayerMove && Team == ETeam.Player)
                return;

            if (state.CurrentPhase != EGamePhase.BossAutoMove && Team == ETeam.Boss)
                return;

            Model.ResetMoves();
        }

        private void HandlePhaseEnded(GameState state)
        {
            if (state.CurrentPhase != EGamePhase.PlayerMove && Team == ETeam.Player)
                return;

            if (state.CurrentPhase != EGamePhase.BossAutoMove && Team == ETeam.Boss)
                return;

            Model.ClearMoves();
        }
    }
}