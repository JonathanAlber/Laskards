using System.Collections.Generic;
using Gameplay.Board;
using Gameplay.CardExecution;
using Gameplay.Movement;
using Gameplay.Units;
using Systems.ObjectPooling;
using Systems.Services;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;

namespace Gameplay.Highlighting
{
    /// <summary>
    /// Visualizes possible move and beat tiles for the currently selected player unit.
    /// Listens to events from <see cref="PlayerMoveController"/> to show and clear previews.
    /// </summary>
    public sealed class PlayerMovePreviewVisualizer : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Color used to indicate possible move tiles.")]
        [SerializeField] private Color previewImageColor = new(0f, 1f, 0f, 0.5f);

        [Tooltip("Color used to indicate possible move tiles in the last row.")]
        [SerializeField] private Color lastRowColor = new(0f, 1f, 0f, 0.5f);

        [Header("Prefabs")]
        [Tooltip("Prefab used to represent a possible move (empty tile).")]
        [SerializeField] private Image possibleMovePreviewPrefab;

        [Tooltip("Prefab used to represent a possible move to an empty tile in the last row.")]
        [SerializeField] private Image possibleLastRowMovePreviewPrefab;

        [Tooltip("Prefab used to represent a possible beat (enemy-occupied tile).")]
        [SerializeField] private Image possibleBeatPreviewPrefab;

        [Header("Parents")]
        [Tooltip("Parent transform under which move preview instances are organized.")]
        [SerializeField] private Transform movePreviewParent;

        [Tooltip("Parent transform under which last row move preview instances are organized.")]
        [SerializeField] private Transform lastRowMovePreviewParent;

        [Tooltip("Parent transform under which beat preview instances are organized.")]
        [SerializeField] private Transform beatPreviewParent;

        private readonly List<Image> _activeMovePreviews = new();
        private readonly List<Image> _activeBeatPreviews = new();
        private readonly List<Image> _activeLastRowMovePreviews = new();

        private HashSetObjectPool<Image> _movePreviewPool;
        private HashSetObjectPool<Image> _beatPreviewPool;
        private HashSetObjectPool<Image> _lastRowMovePreviewPool;

        private GameBoard _board;

        private void Awake()
        {
            if (possibleMovePreviewPrefab == null || possibleBeatPreviewPrefab == null)
            {
                CustomLogger.LogError("Missing one or more prefab references.", this);
                enabled = false;
                return;
            }

            _movePreviewPool = new HashSetObjectPool<Image>(possibleMovePreviewPrefab, movePreviewParent);
            _beatPreviewPool = new HashSetObjectPool<Image>(possibleBeatPreviewPrefab, beatPreviewParent);
            _lastRowMovePreviewPool = new HashSetObjectPool<Image>(possibleLastRowMovePreviewPrefab, lastRowMovePreviewParent);
        }

        private void Start() => _board = ServiceLocator.Get<GameBoard>();

        private void OnEnable()
        {
            PlayerMoveController.OnUnitSelected += HandleUnitSelected;
            PlayerMoveController.OnUnitDeselected += HandleUnitDeselected;
        }

        private void OnDisable()
        {
            PlayerMoveController.OnUnitSelected -= HandleUnitSelected;
            PlayerMoveController.OnUnitDeselected -= HandleUnitDeselected;
        }

        private static void AttachPreviewToTile(Image preview, Tile tile)
        {
            preview.transform.position = tile.transform.position;
        }

        private void HandleUnitSelected(UnitController unit, List<Tile> legalMoves)
        {
            ClearAllPreviews();

            if (unit == null)
            {
                CustomLogger.LogWarning("Received null unit in HandleShowPreview.", this);
                return;
            }

            if (legalMoves == null || legalMoves.Count == 0)
                return;

            if (_movePreviewPool == null || _beatPreviewPool == null || _lastRowMovePreviewPool == null)
            {
                CustomLogger.LogError("Pools are not initialized.", this);
                return;
            }

            if (unit.Model.RemainingMoves <= 0)
                return;

            ETeam unitTeam = unit.Team;

            foreach (Tile tile in legalMoves)
            {
                if (tile == null)
                    continue;

                bool isLastRow = _board.IsLastRow(tile.Row, unitTeam);

                bool isOccupied = tile.IsOccupied();
                if (isOccupied)
                {
                    UnitController occupyingUnit = tile.OccupyingUnit;
                    if (occupyingUnit != null && occupyingUnit.Team != unitTeam)
                    {
                        // Enemy unit preview
                        Image beatPreview = _beatPreviewPool.Get();
                        if (beatPreview == null)
                        {
                            CustomLogger.LogError("Beat preview pool returned null instance.", this);
                            continue;
                        }

                        SetPreviewColor(beatPreview, isLastRow);
                        AttachPreviewToTile(beatPreview, tile);
                        _activeBeatPreviews.Add(beatPreview);
                    }

                    // If there is an enemy unit do not show move preview.
                    continue;
                }

                // Empty tile preview.
                Image movePreview;
                if (isLastRow)
                {
                    movePreview = _lastRowMovePreviewPool.Get();
                    _activeLastRowMovePreviews.Add(movePreview);
                }
                else
                {
                    movePreview = _movePreviewPool.Get();
                    _activeMovePreviews.Add(movePreview);
                }

                SetPreviewColor(movePreview, isLastRow);
                AttachPreviewToTile(movePreview, tile);
            }
        }

        private void SetPreviewColor(Image movePreview, bool isLastRow = false)
        {
            movePreview.color = isLastRow
                ? lastRowColor
                : previewImageColor;
        }

        private void ClearAllPreviews()
        {
            if (_movePreviewPool != null)
                foreach (Image preview in _activeMovePreviews)
                    if (preview != null)
                        _movePreviewPool.Release(preview);

            if (_beatPreviewPool != null)
                foreach (Image preview in _activeBeatPreviews)
                    if (preview != null)
                        _beatPreviewPool.Release(preview);

            if (_lastRowMovePreviewPool != null)
                foreach (Image preview in _activeLastRowMovePreviews)
                    if (preview != null)
                        _lastRowMovePreviewPool.Release(preview);

            _activeMovePreviews.Clear();
            _activeBeatPreviews.Clear();
            _activeLastRowMovePreviews.Clear();
        }

        private void HandleUnitDeselected() => ClearAllPreviews();
    }
}