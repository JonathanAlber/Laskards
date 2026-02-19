namespace Gameplay.Board
{
    /// <summary>
    /// Utility class for formatting grid coordinates into A1 notation.
    /// </summary>
    public static class GridCoordinateFormatter
    {
        /// <summary>
        /// Converts zero-based row/column into A1 grid notation.
        /// </summary>
        /// <param name="row">The zero-based row index.</param>
        /// <param name="column">The zero-based column index.</param>
        /// <returns>The A1-style grid coordinate.</returns>
        /// <example>
        /// Row 3/ Column 0 becomes A4.
        /// </example>
        public static string ToA1(int row, int column) => $"{ColumnToLetters(column)}{row + 1}";

        /// <summary>
        /// Converts a zero-based column index to its corresponding letter(s).
        /// </summary>
        /// <param name="column">The zero-based column index.</param>
        /// <returns>The column represented as letters.</returns>
        /// <example>
        /// Column 0 becomes A, Column 25 becomes Z, Column 26 becomes AA.
        /// </example>
        private static string ColumnToLetters(int column)
        {
            column++;
            string letters = "";

            while (column > 0)
            {
                column--;
                letters = (char)('A' + column % 26) + letters;
                column /= 26;
            }

            return letters;
        }
    }
}