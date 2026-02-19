using System.Collections.Generic;
using Gameplay.Board;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Movement;
using Gameplay.Movement.AI;
using Gameplay.Units;
using Systems.ObjectPooling;
using Systems.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Logging;
using Utility.Raycasting;

namespace Gameplay.Highlighting
{
    /// <summary>
    /// Highlights the selected unit's current tile and the tile currently hovered over during drag.
    /// Also highlights boss spawns and boss AI moves.
    /// </summary>
    public sealed class TileHighlighter : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("GraphicRaycaster used for UI tile raycasts while dragging.")]
        [SerializeField] private GraphicRaycaster uiRaycaster;

        [Tooltip("Parent for pooled highlights (keep this above tiles in hierarchy).")]
        [SerializeField] private Transform highlightParent;

        [Tooltip("Single Image prefab used for all highlights.")]
        [SerializeField] private Image highlightPrefab;

        [Header("Player Colors")]
        [SerializeField] private Color selectedAnchorColor = new(1f, 1f, 0f, 1);
        [SerializeField] private Color dragHoverColor = new(0f, 1f, 1f, 1);
        [SerializeField] private Color moveOriginColor = new(1f, 0.5f, 0f, 1);
        [SerializeField] private Color moveTargetColor = new(0f, 1f, 0f, 1);
        [Space, SerializeField] private Color lastRowColor = new(0f, 1f, 0f, 0.5f);

        [Header("Boss Colors")]
        [SerializeField] private Color bossSpawnColor = new(1f, 0f, 0f, 1);
        [SerializeField] private Color bossMoveOriginColor = new(1f, 0f, 0.7f, 1);
        [SerializeField] private Color bossMoveTargetColor = new(0.8f, 0f, 1f, 1);

        private readonly Dictionary<UnitController, Image> _bossSpawnMap = new();
        private readonly HashSet<Tile> _legalTiles = new();

        private HashSetObjectPool<Image> _pool;

        private Image _bossMoveOrigin;
        private Image _bossMoveTarget;

        private Image _anchor;
        private Image _hover;

        private Image _moveOrigin;
        private Image _moveTarget;

        private UnitController _selectedUnit;
        private GameBoard _board;

        private void Awake()
        {
            _pool = new HashSetObjectPool<Image>(highlightPrefab, highlightParent);
            _board = ServiceLocator.Get<GameBoard>();
        }

        private void OnEnable()
        {
            PlayerMoveController.OnUnitSelected += HandleUnitSelected;
            PlayerMoveController.OnUnitDeselected += HandleUnitDeselected;
            PlayerMoveController.OnMovePerformed += HandleMovePerformed;

            UnitManager.OnBossUnitSpawned += HandleBossUnitSpawned;
            BossAutoMoveController.OnMovePerformed += HandleBossMovePerformed;

            UnitManager.OnUnitDied += HandleUnitDied;

            GameFlowSystem.OnPhaseStarted += OnPhaseStarted;
            GameFlowSystem.OnPhaseEnded += OnPhaseEnded;
        }

        private void OnDisable()
        {
            PlayerMoveController.OnUnitSelected -= HandleUnitSelected;
            PlayerMoveController.OnUnitDeselected -= HandleUnitDeselected;
            PlayerMoveController.OnMovePerformed -= HandleMovePerformed;

            UnitManager.OnBossUnitSpawned -= HandleBossUnitSpawned;
            BossAutoMoveController.OnMovePerformed -= HandleBossMovePerformed;

            UnitManager.OnUnitDied -= HandleUnitDied;

            GameFlowSystem.OnPhaseStarted -= OnPhaseStarted;
            GameFlowSystem.OnPhaseEnded -= OnPhaseEnded;

            ClearSelectionAndHover();
            ClearBossSpawnHighlights();
            ReleaseMoveHighlights();
            ReleaseBossMoveHighlights();
        }

        private static void SetColor(Image img, Color color) => img.color = color;

        private void OnPhaseStarted(GameState state)
        {
            switch (state.CurrentPhase)
            {
                case EGamePhase.BossPrePlay:
                {
                    ClearBossSpawnHighlights();
                    break;
                }
                case EGamePhase.PlayerPrePlay:
                {
                    ReleaseBossMoveHighlights();
                    break;
                }
            }
        }

        private void OnPhaseEnded(GameState state)
        {
            switch (state.CurrentPhase)
            {
                case EGamePhase.PlayerMove or EGamePhase.BossAutoMove:
                {
                    ClearSelectionAndHover();
                    ReleaseMoveHighlights();
                    break;
                }
            }
        }

        private void HandleMovePerformed(Tile origin, Tile target)
        {
            ReleaseMoveHighlights();

            _moveOrigin = GetFromPool(moveOriginColor);
            _moveTarget = GetFromPool(moveTargetColor);

            AttachToTile(_moveOrigin, origin);
            AttachToTile(_moveTarget, target);
        }

        private void ReleaseMoveHighlights()
        {
            if (_moveOrigin != null)
            {
                _pool.Release(_moveOrigin);
                _moveOrigin = null;
            }

            if (_moveTarget != null)
            {
                _pool.Release(_moveTarget);
                _moveTarget = null;
            }
        }

        private void HandleUnitSelected(UnitController unit, List<Tile> legalMoves)
        {
            ClearSelectionAndHover();

            _selectedUnit = unit;
            _legalTiles.Clear();
            if (legalMoves != null)
            {
                foreach (Tile t in legalMoves)
                    if (t != null)
                        _legalTiles.Add(t);
            }

            TryShowAnchorHighlight();

            if (_selectedUnit == null || _selectedUnit.DragDropAdapter == null)
                return;

            _selectedUnit.DragDropAdapter.OnDragging += HandleUnitDragging;
            _selectedUnit.DragDropAdapter.OnDropped += HandleUnitDroppedOrClear;
        }

        private void HandleUnitDeselected() => ClearSelectionAndHover();

        private void HandleUnitDroppedOrClear(UnitController _, PointerEventData __) => ReleaseHover();

        private void TryShowAnchorHighlight()
        {
            if (_selectedUnit == null || _selectedUnit.CurrentTile == null)
                return;

            _anchor = GetFromPool(selectedAnchorColor);
            AttachToTile(_anchor, _selectedUnit.CurrentTile);
        }

        private void HandleUnitDragging(UnitController unit, PointerEventData eventData)
        {
            if (_legalTiles.Count == 0 || !RaycastUtility.TryGetUIElement(uiRaycaster, out Tile tileUnderPointer)
                || tileUnderPointer == null || !_legalTiles.Contains(tileUnderPointer) || unit.Model.RemainingMoves <= 0)
            {
                ReleaseHover();
                return;
            }

            if (_hover == null)
                _hover = GetFromPool(dragHoverColor);

            bool isLastRow = _board.IsLastRow(tileUnderPointer.Row, unit.Team);
            Color color = isLastRow ? lastRowColor : dragHoverColor;
            SetColor(_hover, color);

            AttachToTile(_hover, tileUnderPointer);
        }

        private Image GetFromPool(Color color)
        {
            Image image = _pool.Get();
            if (image == null)
            {
                CustomLogger.LogError("Pool returned null Image.", this);
                return null;
            }

            SetColor(image, color);

            if (image.transform is not RectTransform rectTransform)
            {
                CustomLogger.LogError("Highlight Image is not RectTransform.", this);
                return image;
            }

            Vector2 size = _board.BoardConfiguration.TileSize;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            rectTransform.localScale = Vector3.one;

            return image;
        }

        private void ReleaseHover()
        {
            if (_hover == null)
                return;

            _pool.Release(_hover);
            _hover = null;
        }

        private void ReleaseAnchor()
        {
            if (_anchor == null)
                return;

            _pool.Release(_anchor);
            _anchor = null;
        }

        private void ClearSelectionAndHover()
        {
            if (_selectedUnit != null)
            {
                _selectedUnit.DragDropAdapter.OnDragging -= HandleUnitDragging;
                _selectedUnit.DragDropAdapter.OnDropped -= HandleUnitDroppedOrClear;
            }

            ReleaseHover();
            ReleaseAnchor();

            _legalTiles.Clear();
            _selectedUnit = null;
        }

        private static void AttachToTile(Image img, Tile tile)
        {
            if (img == null)
            {
                CustomLogger.LogWarning("Tried to attach null Image to tile.", null);
                return;
            }

            if (tile == null)
            {
                CustomLogger.LogWarning("Tried to attach highlight to null tile.", null);
                return;
            }

            Transform t = img.transform;
            t.SetParent(tile.transform, false);
            t.localPosition = Vector3.zero;
        }

        private void HandleBossUnitSpawned(UnitController boss)
        {
            if (boss == null)
            {
                CustomLogger.LogWarning("Received null boss unit.", this);
                return;
            }

            if (boss.CurrentTile == null)
            {
                CustomLogger.LogWarning("Boss unit has no current tile.", this);
                return;
            }

            // Remove old spawn highlight if it existed
            if (_bossSpawnMap.TryGetValue(boss, out Image old))
            {
                _pool.Release(old);
                _bossSpawnMap.Remove(boss);
            }

            Image highlight = GetFromPool(bossSpawnColor);
            AttachToTile(highlight, boss.CurrentTile);

            _bossSpawnMap[boss] = highlight;
        }

        private void ClearBossSpawnHighlights()
        {
            foreach (Image img in _bossSpawnMap.Values)
                _pool.Release(img);

            _bossSpawnMap.Clear();
        }

        private void HandleBossMovePerformed(UnitController unit, Tile origin, Tile target)
        {
            if (_bossSpawnMap.TryGetValue(unit, out Image spawnImg))
            {
                _pool.Release(spawnImg);
                _bossSpawnMap.Remove(unit);
            }

            ReleaseBossMoveHighlights();

            _bossMoveOrigin = GetFromPool(bossMoveOriginColor);
            _bossMoveTarget = GetFromPool(bossMoveTargetColor);

            AttachToTile(_bossMoveOrigin, origin);
            AttachToTile(_bossMoveTarget, target);
        }

        private void ReleaseBossMoveHighlights()
        {
            if (_bossMoveOrigin != null)
            {
                _pool.Release(_bossMoveOrigin);
                _bossMoveOrigin = null;
            }

            if (_bossMoveTarget != null)
            {
                _pool.Release(_bossMoveTarget);
                _bossMoveTarget = null;
            }
        }

        private void HandleUnitDied(UnitController unit)
        {
            if (unit == null)
                return;

            // Clear boss spawn highlight for this unit
            if (_bossSpawnMap.TryGetValue(unit, out Image spawnImg))
            {
                _pool.Release(spawnImg);
                _bossSpawnMap.Remove(unit);
            }

            // Clear move highlights if this unit was involved
            ReleaseMoveHighlights();
            ReleaseBossMoveHighlights();

            // Clear selection/hover if this unit was the selected one
            if (_selectedUnit == unit)
                ClearSelectionAndHover();
        }
    }
}