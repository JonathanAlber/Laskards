using UnityEngine;
using Gameplay.Board;
using Gameplay.Cards;
using UnityEngine.UI;
using Gameplay.Player;
using Utility.Logging;
using Systems.Services;
using Utility.Raycasting;
using Gameplay.Cards.Modifier.Data.Runtime;

namespace Gameplay.Highlighting
{
    /// <summary>
    /// Handles visualization for dragging tile-targeting action cards, such as highlighting valid target tiles.
    /// </summary>
    public class TileCardDragVisualizationHandler : ActionCardDragHighlighter<Tile>
    {
        [Header("Tile Card")]
        [Tooltip("The image that is used to highlight hovered tiles.")]
        [SerializeField] private Image highlightImage;

        private ETileDragHighlightMode _highlightMode;

        private void Awake() => SetHighlightImageActive(false);

        protected override void HandleDragStarted(CardController card)
        {
            SetHighlightImageActive(false);

            if (card == null)
            {
                CustomLogger.LogWarning("Dragged card is null.", this);
                return;
            }

            if (card is not ActionCardController actionCard)
                return;

            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            if (!player.HasEnoughEnergy(card.Model))
                return;

            if (!ServiceLocator.TryGet(out GameBoard board))
                return;

            card.View.FadeCard(true);

            switch (actionCard.TypedModel.ModifierState)
            {
                case BarrierModifierRuntimeState:
                {
                    _highlightMode = ETileDragHighlightMode.HighlightFree;
                    break;
                }
                case ClearTileModifierRuntimeState:
                {
                    _highlightMode = ETileDragHighlightMode.HighlightNonUnits;
                    break;
                }
                default:
                {
                    CustomLogger.LogWarning("Unhandled tile action card modifier type " +
                                            $"{actionCard.TypedModel.ModifierState.GetType().Name} " +
                                            $"in {nameof(TileCardDragVisualizationHandler)}.", this);
                    _highlightMode = ETileDragHighlightMode.None;
                    return;
                }
            }

            ValidModifiables.Clear();
            foreach (Tile tile in board.Tiles)
            {
                if (tile == null)
                {
                    CustomLogger.LogWarning("Encountered null unit while gathering valid buff targets.", this);
                    continue;
                }

                bool isOccupied = tile.IsOccupied();
                switch (_highlightMode)
                {
                    case ETileDragHighlightMode.HighlightFree when isOccupied:
                    case ETileDragHighlightMode.HighlightOccupied when !isOccupied:
                        continue;
                    case ETileDragHighlightMode.HighlightNonUnits:
                    {
                        if (!isOccupied)
                        {
                            ValidModifiables.Add(tile);
                            break;
                        }

                        if (tile.OccupyingUnit == null)
                            ValidModifiables.Add(tile);

                        break;
                    }
                    case ETileDragHighlightMode.HighlightAll:
                    default:
                        ValidModifiables.Add(tile);
                        break;
                }
            }
        }

        protected override void HandleDragging(CardController card)
        {
            if (_highlightMode == ETileDragHighlightMode.None)
                return;

            if (card == null)
            {
                CustomLogger.LogWarning("Dragged card is null.", this);
                return;
            }

            if (!RaycastUtility.TryGetUIElement(uiRaycaster, out Tile tile) || tile == null
                || !ValidModifiables.Contains(tile))
            {
                SetHighlightImageActive(false);
                return;
            }

            highlightImage.transform.position = tile.transform.position;
            SetHighlightImageActive(true);
        }

        protected override void HandleDragEnded(CardController card)
        {
            if (card != null)
                card.View.FadeCard(false);

            _highlightMode = ETileDragHighlightMode.None;
            SetHighlightImageActive(false);
            ValidModifiables.Clear();
        }

        private void SetHighlightImageActive(bool isActive) => highlightImage.gameObject.SetActive(isActive);
    }
}