using UnityEngine;
using Gameplay.Board;
using Gameplay.Cards;
using UnityEngine.UI;
using Gameplay.Player;
using Utility.Logging;
using Systems.Services;
using Utility.Raycasting;
using Systems.ObjectPooling;
using Gameplay.CardExecution;
using System.Collections.Generic;

namespace Gameplay.Highlighting
{
    /// <summary>
    /// Handles visualization for dragging unit cards, such as highlighting valid drop zones.
    /// </summary>
    public class UnitCardDragVisualizationHandler : CardDragHighlighter
    {
        [Header("Unit Card Drag Visualization Handler")]
        [Tooltip("Prefab used to highlight the current tile under the pointer.")]
        [SerializeField] private Image highlight;

        [Header("Colors")]
        [Tooltip("Color used to indicate possible tiles to place the card on.")]
        [SerializeField] private Color previewImageColor = new(0f, 1f, 0f, 0.5f);

        [Header("Prefabs")]
        [Tooltip("Prefab used to represent a possible move (empty tile).")]
        [SerializeField] private Image possibleMovePreviewPrefab;

        [Header("Parents")]
        [Tooltip("Parent transform under which move preview instances are organized.")]
        [SerializeField] private Transform movePreviewParent;

        private readonly List<Tile> _validTiles = new();
        private readonly List<Image> _activeMovePreviews = new();

        private HashSetObjectPool<Image> _movePreviewPool;

        private void Awake()
        {
            if (possibleMovePreviewPrefab == null || highlight == null)
            {
                CustomLogger.LogError("Missing one or more prefab references.", this);
                enabled = false;
                return;
            }

            _movePreviewPool = new HashSetObjectPool<Image>(possibleMovePreviewPrefab, movePreviewParent);
        }

        protected override void HandleDragStarted(CardController card)
        {
            ClearAllPreviews();

            if (card == null)
            {
                CustomLogger.LogWarning("Dragged card is null.", this);
                return;
            }

            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            if (!ServiceLocator.TryGet(out GameBoard gameBoard))
                return;

            if (!ServiceLocator.TryGet(out UnitPlacementValidator placementValidator))
                return;

            if (!player.HasEnoughEnergy(card.Model))
                return;

            card.View.FadeCard(true);

            // Iterate through the allowed rows and columns to find valid tiles
            _validTiles.Clear();
            int playerRowsFromBottom = placementValidator.Deployment.PlayerRowsFromBottom;
            int totalColumns = gameBoard.BoardConfiguration.Columns;
            for (int row = 0; row < playerRowsFromBottom; row++)
            {
                for (int col = 0; col < totalColumns; col++)
                {
                    Tile tile = gameBoard.GetTile(row, col);
                    if (tile != null && placementValidator.CanPlace(player, tile, out _))
                        _validTiles.Add(tile);
                }
            }

            // Create and position previews for each valid tile
            foreach (Tile tile in _validTiles)
            {
                Image movePreview = _movePreviewPool.Get();
                movePreview.color = previewImageColor;
                movePreview.transform.position = tile.transform.position;
                _activeMovePreviews.Add(movePreview);
            }
        }

        protected override void HandleDragging(CardController card)
        {
            if (card == null)
            {
                CustomLogger.LogWarning("Dragged card is null.", this);
                return;
            }

            if (!RaycastUtility.TryGetUIElement(uiRaycaster, out Tile tileUnderPointer) || tileUnderPointer == null
                || !_validTiles.Contains(tileUnderPointer))
            {
                SetHighlightActivity(false);
                return;
            }

            SetHighlightActivity(true);
            highlight.transform.position = tileUnderPointer.transform.position;
        }

        protected override void HandleDragEnded(CardController card)
        {
            card.View.FadeCard(false);

            ClearAllPreviews();
            _validTiles.Clear();
        }

        protected override bool ValidateCard(CardController card) => card is UnitCardController;

        private void ClearAllPreviews()
        {
            SetHighlightActivity(false);
            ClearMovePool();
        }

        private void SetHighlightActivity(bool state) => highlight.gameObject.SetActive(state);

        private void ClearMovePool()
        {
            if (_movePreviewPool != null)
                foreach (Image preview in _activeMovePreviews)
                    if (preview != null)
                        _movePreviewPool.Release(preview);

            _activeMovePreviews.Clear();
        }
    }
}