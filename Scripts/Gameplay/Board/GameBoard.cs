using Gameplay.Board.Data;
using Gameplay.CardExecution;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Systems.Services;
using UnityEngine;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Board
{
    /// <summary>
    /// Manages the game board, including tiles and their effects.
    /// </summary>
    public sealed class GameBoard : GameServiceBehaviour, IPhaseSubscriber
    {
        [field: Tooltip("Configuration data scriptable object for the board layout and tiles.")]
        [field: SerializeField] public BoardConfiguration BoardConfiguration { get; private set; }

        [Tooltip("Optional parent object for all tiles.")]
        [SerializeField] private Transform tileParent;
        [SerializeField] private RectTransform backdropShadowRect;

        public FlattenedArray<Tile> Tiles { get; private set; }

        public EGamePhase[] SubscribedPhases => new[]
        {
            EGamePhase.PlayerPrePlay,
            EGamePhase.BossPrePlay
        };

        protected override void Awake()
        {
            base.Awake();

            if (!ServiceLocator.TryGet(out GameFlowSystem flow))
                return;

            InstantiateBoard();

            foreach (EGamePhase phase in SubscribedPhases)
                flow.RegisterPhaseSubscriber(phase, this);
        }

        /// <summary>
        /// Checks if the specified row index is the last row for the given team.
        /// </summary>
        /// <param name="rowIndex">The row index to check.</param>
        /// <param name="team">The team to determine the last row for.</param>
        /// <returns><c>true</c> if the row index is the last row for the team; otherwise, <c>false</c>.</returns>
        public bool IsLastRow(int rowIndex, ETeam team)
        {
            int lastRow = BoardConfiguration.Rows - 1;
            bool enteredLastRow = team == ETeam.Player
                ? rowIndex == lastRow
                : rowIndex == 0;

            return enteredLastRow;
        }

        /// <summary>
        /// Gets the tile at the specified grid coordinates.
        /// </summary>
        /// <param name="row">The row index. Would be the A of A1 in chess notation.</param>
        /// <param name="col">The column index. Would be the 1 of A1 in chess notation.</param>
        /// <returns></returns>
        public Tile GetTile(int row, int col)
        {
            if (Tiles == null)
            {
                CustomLogger.LogError("Unable to get tile. Tiles array is null.", this);
                return null;
            }

            int cols = BoardConfiguration.Columns;
            int rows = BoardConfiguration.Rows;

            if (col < 0 || col >= cols || row < 0 || row >= rows)
            {
                CustomLogger.LogError($"Tile coordinates out of bounds: ({row}, {col})", this);
                return null;
            }

            return Tiles.Get(col, row);
        }

        public void OnPhaseStarted(GameState state, GameFlowSystem flow)
        {
            foreach (Tile tile in Tiles)
                tile.TickEffects();

            flow.MarkSubscriberDone(this);
        }

        public void OnPhaseEnded(GameState state) { }

        /// <summary>
        /// Instantiates the board tiles based on configuration, keeping them centered and evenly spaced in UI space.
        /// </summary>
        private void InstantiateBoard()
        {
            if (BoardConfiguration == null || BoardConfiguration.TilePrefab == null)
            {
                CustomLogger.LogError("Missing configuration or tile prefab reference!", this);
                return;
            }

            int rows = BoardConfiguration.Rows;
            int cols = BoardConfiguration.Columns;
            Tiles = new FlattenedArray<Tile>(cols, rows);

            Vector2 tileSize = BoardConfiguration.TileSize;
            Vector2 spacingOffset = BoardConfiguration.TileSpacingOffset;

            // Actual spacing between centers of tiles = tile size + offset
            Vector2 effectiveSpacing = new(tileSize.x + spacingOffset.x, tileSize.y + spacingOffset.y);

            // Total grid width/height in local canvas space
            float totalWidth = cols * effectiveSpacing.x;
            float totalHeight = rows * effectiveSpacing.y;

            // Offset to keep board centered around (0,0)
            Vector2 startOffset = new(-totalWidth / 2f + effectiveSpacing.x / 2f,
                               -totalHeight / 2f + effectiveSpacing.y / 2f);

            Color colorA = BoardConfiguration.PrimaryColor;
            Color colorB = BoardConfiguration.SecondaryColor;

            RectTransform parentRect = tileParent as RectTransform;
            if (parentRect == null)
            {
                CustomLogger.LogError("Tile parent must be a RectTransform for UI-based tiles!", this);
                return;
            }

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Compute local anchored position
                    float x = startOffset.x + col * effectiveSpacing.x;
                    float y = startOffset.y + row * effectiveSpacing.y;

                    Tile tile = Instantiate(BoardConfiguration.TilePrefab, tileParent);
                    tile.name = $"Tile_{row}_{col}";

                    RectTransform rt = tile.transform as RectTransform;
                    if (rt != null)
                    {
                        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(x, y);
                        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tileSize.x);
                        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tileSize.y);
                    }

                    bool isEven = (row + col) % 2 == 0;
                    tile.SetColor(isEven ? colorA : colorB);

                    tile.Row = row;
                    tile.Column = col;

                    Tiles.Set(col, row, tile);
                }
            }
        }
    }
}