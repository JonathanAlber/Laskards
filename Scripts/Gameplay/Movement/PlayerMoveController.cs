using System;
using System.Collections.Generic;
using Gameplay.Board;
using Gameplay.CardExecution;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Units;
using Gameplay.Units.Movement;
using Interaction;
using Systems.Input;
using Systems.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Raycasting;
using UnityEngine.InputSystem;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Movement
{
    /// <summary>
    /// Handles the player's move phase.
    /// Allows selecting a unit and moving it via clicks or drag-and-drop.
    /// </summary>
    public sealed class PlayerMoveController : GameServiceBehaviour
    {
        /// <summary>
        /// Fired when the move is finished or selection is cleared.
        /// </summary>
        public static event Action OnUnitDeselected;

        /// <summary>
        /// Fired when a move has been successfully performed.
        /// </summary>
        public static event Action<Tile, Tile> OnMovePerformed;

        /// <summary>
        /// Fired when a unit enters the last row of the opponent.
        /// </summary>
        public static event Action<UnitController> OnUnitEnteredLastRow;

        /// <summary>
        /// Fired when a unit is selected and possible move/beat tiles should be shown.
        /// </summary>
        public static event Action<UnitController, List<Tile>> OnUnitSelected;

        [Header("References")]
        [SerializeField] private GraphicRaycaster uiRaycaster;

        private GameBoard _board;
        private UnitController _selectedUnit;
        private List<Tile> _cachedLegalMoves;
        private InputManager _inputManager;

        protected override void Awake()
        {
            base.Awake();

            _board = ServiceLocator.Get<GameBoard>();

            GameFlowSystem.OnPhaseStarted += OnPhaseStarted;
            GameFlowSystem.OnPhaseEnded += OnPhaseEnded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameFlowSystem.OnPhaseStarted -= OnPhaseStarted;
            GameFlowSystem.OnPhaseEnded -= OnPhaseEnded;

            if (_inputManager != null)
                _inputManager.InputActions.Permanent.Click.performed -= OnGlobalClick;
        }

        private void OnPhaseStarted(GameState state)
        {
            if (state.CurrentPhase != EGamePhase.PlayerMove)
                return;

            EnableInput();
        }

        private void OnPhaseEnded(GameState state)
        {
            if (state.CurrentPhase != EGamePhase.PlayerMove)
                return;

            DisableInput();
            ClearSelection();
        }

        private void EnableInput()
        {
            HookAllPlayerUnits(true);
            HookAllBossUnits(true);
            HookAllTiles(true);

            if (ServiceLocator.TryGet(out _inputManager))
                _inputManager.InputActions.Permanent.Click.performed += OnGlobalClick;
        }

        private void DisableInput()
        {
            HookAllPlayerUnits(false);
            HookAllBossUnits(false);
            HookAllTiles(false);

            if (_inputManager != null)
                _inputManager.InputActions.Permanent.Click.performed -= OnGlobalClick;
        }

        private void OnGlobalClick(InputAction.CallbackContext _)
        {
            if (_inputManager.IsCursorOverGameObject)
                return;

            if (_selectedUnit != null)
                ClearSelection();
        }

        private void HookAllPlayerUnits(bool enable)
        {
            if (!ServiceLocator.TryGet(out UnitManager unitManager))
                return;

            IReadOnlyList<UnitController> playerUnits = unitManager.PlayerUnits;
            foreach (UnitController unit in playerUnits)
            {
                if (unit == null)
                {
                    CustomLogger.LogWarning("Null unit in player units list!", this);
                    continue;
                }

                if (unit.Model == null)
                {
                    CustomLogger.LogWarning($"Unit {unit.name} is missing model!", this);
                    return;
                }

                if (!unit.Model.IsAlive)
                {
                    CustomLogger.LogWarning($"Unit {unit.name} is not alive!", this);
                    continue;
                }

                if (enable)
                {
                    unit.DragDropAdapter.OnClicked += OnUnitClicked;
                    unit.DragDropAdapter.OnDragStarted += OnUnitDragStarted;
                    unit.DragDropAdapter.OnDropped += OnUnitDropped;
                }
                else
                {
                    unit.DragDropAdapter.OnClicked -= OnUnitClicked;
                    unit.DragDropAdapter.OnDragStarted -= OnUnitDragStarted;
                    unit.DragDropAdapter.OnDropped -= OnUnitDropped;
                }
            }
        }

        private void HookAllBossUnits(bool enable)
        {
            if (!ServiceLocator.TryGet(out UnitManager unitManager))
                return;

            IReadOnlyList<UnitController> bossUnits = unitManager.BossUnits;

            foreach (UnitController unit in bossUnits)
            {
                if (unit == null)
                {
                    CustomLogger.LogWarning("Null unit in boss units list!", this);
                    continue;
                }

                if (unit.Model == null)
                {
                    CustomLogger.LogWarning($"Unit {unit.name} is missing model!", this);
                    return;
                }

                if (!unit.Model.IsAlive)
                {
                    CustomLogger.LogWarning($"Unit {unit.name} is not alive!", this);
                    continue;
                }

                if (enable)
                    unit.DragDropAdapter.OnClicked += OnBossUnitClicked;
                else
                    unit.DragDropAdapter.OnClicked -= OnBossUnitClicked;
            }
        }

        private void HookAllTiles(bool enable)
        {
            FlattenedArray<Tile> tiles = _board.Tiles;
            foreach (Tile tile in tiles)
            {
                if (enable)
                    tile.ClickResponder.OnClicked += OnTileClicked;
                else
                    tile.ClickResponder.OnClicked -= OnTileClicked;
            }
        }

        private void OnUnitClicked(UnitController unit) => SelectUnitByClick(unit);

        private void OnUnitDragStarted(UnitController unit)
        {
            unit.transform.SetAsLastSibling();
            SelectUnitByClick(unit);
        }

        private void OnBossUnitClicked(UnitController bossUnit)
        {
            if (bossUnit == null)
            {
                CustomLogger.LogWarning("Clicked boss unit is null.", this);
                return;
            }

            if (_selectedUnit == null)
                return;

            Tile bossTile = bossUnit.CurrentTile;
            if (bossTile != null)
                TryMoveSelectedTo(bossTile);
        }

        private void OnTileClicked(ClickResponder _, PointerEventData __)
        {
            if (_selectedUnit == null)
                return;

            if (!RaycastUtility.TryGetUIElement(uiRaycaster, out Tile tile))
            {
                CustomLogger.LogWarning("No tile found under pointer on click.", this);
                return;
            }

            TryMoveSelectedTo(tile);
        }

        private void OnUnitDropped(UnitController unit, PointerEventData eventData)
        {
            if (unit == null)
            {
                CustomLogger.LogWarning("Dropped unit is null.", this);
                return;
            }

            if (_selectedUnit != unit)
                SelectUnit(unit);

            if (!RaycastUtility.TryGetUIElement(uiRaycaster, out Tile tile))
            {
                unit.AttachToTile(unit.CurrentTile);
                ClearSelection();
                CustomLogger.LogWarning("No tile found under pointer on drop.", this);
                return;
            }

            if (TryMoveSelectedTo(tile))
                return;

            unit.AttachToTile(unit.CurrentTile);
            ClearSelection();
        }

        private void SelectUnitByClick(UnitController unit)
        {
            if (unit == null)
            {
                CustomLogger.LogWarning("Attempted to select a null unit.", this);
                return;
            }

            if (unit.Team != ETeam.Player)
            {
                CustomLogger.LogWarning("Attempted to select a non-player unit.", this);
                return;
            }

            SelectUnit(unit);
        }

        private void SelectUnit(UnitController unit)
        {
            _selectedUnit = unit;
            _cachedLegalMoves = ComputeLegalMoves(unit);

            if (unit.Model != null)
                OnUnitSelected?.Invoke(unit, _cachedLegalMoves);
            else
                OnUnitDeselected?.Invoke();
        }

        private void ClearSelection()
        {
            _selectedUnit = null;
            _cachedLegalMoves = null;
            OnUnitDeselected?.Invoke();
        }

        private List<Tile> ComputeLegalMoves(UnitController unit)
        {
            if (unit == null || unit.Model == null)
            {
                CustomLogger.LogWarning("Cannot compute legal moves for null unit or unit with null model.", this);
                return new List<Tile>();
            }

            UnitMovementDefinition def = StandardUnitMovementLibrary.GetDefinition(unit.Model.UnitType);

            int rows = _board.BoardConfiguration.Rows;
            int cols = _board.BoardConfiguration.Columns;

            return BoardMovementResolver.GetLegalMoves(_board, unit, def, rows, cols);
        }

        private bool TryMoveSelectedTo(Tile target)
        {
            if (target == null)
            {
                CustomLogger.LogWarning("No target tile specified for movement.", this);
                return false;
            }

            if (_selectedUnit == null)
            {
                CustomLogger.Log("No unit selected for movement.", this);
                return false;
            }

            if (_cachedLegalMoves == null || _cachedLegalMoves.Count == 0)
            {
                CustomLogger.Log("No legal moves cached for the selected unit.", this);
                return false;
            }

            bool legal = false;
            foreach (Tile tile in _cachedLegalMoves)
            {
                if (tile != target)
                    continue;

                legal = true;
                break;
            }

            if (!legal)
            {
                CustomLogger.Log("Attempted to move to an illegal tile at " +
                                 $"{GridCoordinateFormatter.ToA1(target.Row, target.Column)}.", this);
                return false;
            }

            Tile origin = _selectedUnit.CurrentTile;
            bool success = TryPerformMove(_selectedUnit, target);

            if (success)
                OnMovePerformed?.Invoke(origin, target);

            ClearSelection();
            return success;
        }

        private bool TryPerformMove(UnitController unit, Tile target)
        {
            if (unit == null)
            {
                CustomLogger.LogWarning("PerformMove called with null unit.", this);
                return false;
            }

            if (target == null)
            {
                CustomLogger.LogWarning("PerformMove called with null target.", this);
                return false;
            }

            Tile origin = unit.CurrentTile;
            if (origin == null)
            {
                CustomLogger.LogWarning($"Unit {unit.name} has no origin tile assigned.", this);
                return false;
            }

            if (target == origin)
            {
                CustomLogger.LogWarning($"Unit {unit.name} attempted to move to its current tile ({target.name}).", this);
                return false;
            }

            if (unit.Model == null)
            {
                CustomLogger.LogWarning($"Unit {unit.name} has null model during move.", this);
                return false;
            }

            if (unit.Model.RemainingMoves <= 0)
            {
                CustomLogger.LogWarning($"Unit {unit.name} has no moves left for this phase.", this);
                unit.AttachToTile(origin);
                return false;
            }

            if (target.IsOccupied())
            {
                UnitController defender = target.OccupyingUnit;
                if (defender == null)
                {
                    CustomLogger.LogWarning("Target tile is occupied but has no occupying unit.", this);
                    return false;
                }

                if (defender.Team == unit.Team)
                {
                    CustomLogger.LogWarning("Attempted to attack a unit on the same team.", this);
                    return false;
                }

                if (!defender.CanBeAttacked())
                {
                    CustomLogger.LogWarning("Defender unit cannot be attacked due to active effects.", this);
                    return false;
                }

                defender.ApplyDamage(unit.Model.GetFinalDamage(), unit);
                if (defender.Model.IsAlive)
                {
                    CustomLogger.Log($"Defender {defender.name} survived the attack. Move aborted.", this);

                    unit.Model.ConsumeMove();

                    if (!unit.Model.DecreaseLifetime())
                        return false;

                    CustomLogger.Log($"Unit {unit.name} expired due to lifetime reaching zero.", this);
                    unit.Die();

                    return false;
                }
            }

            if (!unit.Model.IsAlive)
            {
                CustomLogger.Log($"Unit at ({GridCoordinateFormatter.ToA1(origin.Row, origin.Column)}) " +
                                 "died before completing its move.", this);
                return false;
            }

            origin.RemoveUnit(unit);
            target.PlaceUnit(unit);
            unit.AttachToTile(target);

            unit.Model.ConsumeMove();

            if (_board.IsLastRow(target.Row, unit.Team))
            {
                OnUnitEnteredLastRow?.Invoke(unit);
                unit.Die();
                return true;
            }

            if (!unit.Model.DecreaseLifetime())
                return true;

            CustomLogger.Log($"Unit {unit.name} expired due to lifetime reaching zero.", this);
            unit.Die();

            return true;
        }
    }
}