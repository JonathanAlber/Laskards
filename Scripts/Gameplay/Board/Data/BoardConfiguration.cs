using UnityEngine;

namespace Gameplay.Board.Data
{
    /// <summary>
    /// Configuration data for the game board, including dimensions, positioning and tile prefab.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardConfiguration", menuName = "ScriptableObjects/Board/Board Configuration")]
    public class BoardConfiguration : ScriptableObject
    {
        [field: Header("Board Layout")]
        [field: SerializeField, Tooltip("Number of rows in the board grid.")]
        public int Rows { get; private set; } = 4;

        [field: SerializeField, Tooltip("Number of columns in the board grid.")]
        public int Columns { get; private set; } = 8;

        [field: SerializeField, Tooltip("Prefab used to instantiate individual tiles on the board.")]
        public Tile TilePrefab { get; private set; }

        [field: Header("Board Positioning")]
        [field: SerializeField, Tooltip("Additional offset between each tile (X for horizontal spacing, Y for vertical spacing).")]
        public Vector2 TileSpacingOffset { get; private set; } = Vector2.zero;

        [field: Header("Tile Settings")]
        [field: SerializeField, Tooltip("Uniform scale multiplier applied to each instantiated tile.")]
        public Vector2 TileSize { get; private set; }  = Vector2.one;

        [field: SerializeField, Tooltip("Primary tile color (like white in chess).")]
        public Color PrimaryColor { get; private set; } = Color.white;

        [field: SerializeField, Tooltip("Secondary tile color (like black in chess).")]
        public Color SecondaryColor { get; private set; } = Color.black;
    }
}