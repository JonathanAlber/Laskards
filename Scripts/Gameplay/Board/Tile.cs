using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Units;
using Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Logging;

namespace Gameplay.Board
{
    /// <summary>
    /// Represents a single board tile that can hold effects or status modifiers.
    /// </summary>
    [RequireComponent(typeof(Image), typeof(ClickResponder))]
    public class Tile : MonoBehaviour, IModifiable<TileEffect>
    {
        /// <summary>
        /// Event triggered when a tile is clicked.
        /// </summary>
        public static event Action<Tile> OnTileClicked;

        public List<TileEffect> ActiveEffects { get; } = new();

        /// <summary>
        /// Row index of this tile.
        /// </summary>
        public int Row { get; internal set; }

        /// <summary>
        /// Column index of this tile.
        /// </summary>
        public int Column { get; internal set; }

        /// <summary>
        /// The unit currently occupying this tile, or null if empty.
        /// </summary>
        public UnitController OccupyingUnit { get; private set; }

        /// <summary>
        /// Click responder component for handling user interactions.
        /// </summary>
        public ClickResponder ClickResponder { get; private set; }

        private Image _image;

        private void Awake()
        {
            _image = GetComponentInChildren<Image>();
            ClickResponder = GetComponent<ClickResponder>();
            ClickResponder.OnClicked += HandleClick;
        }

        /// <summary>
        /// Sets the color of the tile sprite or material.
        /// </summary>
        /// <param name="color">The color to apply.</param>
        public void SetColor(Color color) => _image.color = color;

        /// <summary>
        /// Checks if the tile is currently occupied by any entity (unit, barrier, etc.).
        /// </summary>
        public bool IsOccupied() => OccupyingUnit != null || ActiveEffects.Any(tileEffect => tileEffect.OccupiesTile);

        private void OnDestroy()
        {
            if (OccupyingUnit != null)
                OccupyingUnit.OnUnitDied -= ClearOccupyingUnit;

            ClickResponder.OnClicked -= HandleClick;
        }

        /// <summary>
        /// Places a unit on this tile if it is unoccupied.
        /// </summary>
        /// <param name="newUnit">The unit to place on the tile.</param>
        public void PlaceUnit(UnitController newUnit)
        {
            if (newUnit == null)
            {
                CustomLogger.LogWarning("Attempted to place a null unit on the tile.", this);
                return;
            }

            if (IsOccupied())
            {
                CustomLogger.LogWarning("Attempted to place a unit on an occupied tile.", this);
                return;
            }

            OccupyingUnit = newUnit;
            OccupyingUnit.OnUnitDied += ClearOccupyingUnit;
        }

        /// <summary>
        /// Removes the specified unit from this tile if it is the occupying unit.
        /// </summary>
        /// <param name="unit">The unit to remove from the tile.</param>
        public void RemoveUnit(UnitController unit)
        {
            if (unit == null)
            {
                CustomLogger.LogWarning("Attempted to remove a null unit from the tile.", this);
                return;
            }

            if (OccupyingUnit != unit)
            {
                CustomLogger.LogWarning("Attempted to remove a unit that does not occupy this tile.", this);
                return;
            }

            if (OccupyingUnit == null)
            {
                CustomLogger.LogWarning("Occupying unit is already null.", this);
                return;
            }

            OccupyingUnit.OnUnitDied -= ClearOccupyingUnit;
            OccupyingUnit = null;
        }

        public void AddEffect(TileEffect effect)
        {
            if (effect == null)
                return;

            ActiveEffects.Add(effect);
            effect.Apply();
        }

        public void RemoveEffect(TileEffect effect)
        {
            if (effect == null)
                return;

            ActiveEffects.Remove(effect);
            effect.Revert();
        }

        public void ClearEffects()
        {
            for (int i = ActiveEffects.Count - 1; i >= 0; i--)
                RemoveEffect(ActiveEffects[i]);
        }

        public void AddEffect(IEffect effect)
        {
            if (effect is TileEffect tileEffect)
                AddEffect(tileEffect);
        }

        public void RemoveEffect(IEffect effect)
        {
            if (effect is TileEffect tileEffect)
                RemoveEffect(tileEffect);
        }

        public void TickEffects()
        {
            if (ActiveEffects.Count == 0)
                return;

            List<TileEffect> expired = new();

            foreach (TileEffect effect in ActiveEffects)
            {
                if (effect.CreatorTeam == ETeam.Boss && GameFlowSystem.CurrentPhase == EGamePhase.PlayerPrePlay)
                    continue;

                if (effect.CreatorTeam == ETeam.Player && GameFlowSystem.CurrentPhase == EGamePhase.BossPrePlay)
                    continue;

                effect.Tick();
                if (effect.IsExpired)
                    expired.Add(effect);
            }

            foreach (TileEffect e in expired)
            {
                e.Revert();
                ActiveEffects.Remove(e);
            }
        }

        public bool HasMaxEffectsReached() => false;

        public bool ValidatePlayer(ETeam team) => true;

        private void ClearOccupyingUnit(UnitController unit)
        {
            if (OccupyingUnit != unit)
                return;

            unit.OnUnitDied -= ClearOccupyingUnit;
            OccupyingUnit = null;
        }

        private void HandleClick(ClickResponder clickResponder, PointerEventData _)
        {
            if (!InteractionContext.AllowTileClicks)
                return;

            OnTileClicked?.Invoke(this);
        }
    }
}