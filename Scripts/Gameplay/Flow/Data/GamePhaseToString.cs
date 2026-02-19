namespace Gameplay.Flow.Data
{
    /// <summary>
    /// Extension methods for converting <see cref="EGamePhase"/> to readable strings.
    /// </summary>
    public static class GamePhaseToString
    {
        /// <summary>
        /// Converts an <see cref="EGamePhase"/> enum value to an understandable string.
        /// </summary>
        /// <param name="phase">The game phase to convert.</param>
        /// <returns>A readable string representation of the game phase.</returns>
        public static string ToReadableString(this EGamePhase phase)
        {
            return phase switch
            {
                EGamePhase.None => "None",
                EGamePhase.PlayerPrePlay => "Player – Preparation",
                EGamePhase.PlayerDraw => "Player – Draw",
                EGamePhase.PlayerPlay => "Player – Card Play",
                EGamePhase.PlayerMove => "Player – Move",
                EGamePhase.BossPrePlay => "Boss – Preparation",
                EGamePhase.BossPlay => "Boss – Card Play",
                EGamePhase.BossAutoMove => "Boss – Move",
                _ => "Unknown Phase"
            };
        }

        /// <summary>
        /// Converts an <see cref="EGamePhase"/> enum value to a shorter understandable string.
        /// </summary>
        /// <param name="phase">The game phase to convert.</param>
        /// <returns>A short readable string representation of the game phase.</returns>
        public static string ToShortReadableString(this EGamePhase phase)
        {
            return phase switch
            {
                EGamePhase.None => "None",
                EGamePhase.PlayerPrePlay => "Preparation",
                EGamePhase.PlayerDraw => "Draw",
                EGamePhase.PlayerPlay => "Play",
                EGamePhase.PlayerMove => "Move",
                EGamePhase.BossPrePlay => "Boss Preparation",
                EGamePhase.BossPlay => "Boss Play",
                EGamePhase.BossAutoMove => "Boss Move",
                _ => "Unknown Phase"
            };
        }
    }
}