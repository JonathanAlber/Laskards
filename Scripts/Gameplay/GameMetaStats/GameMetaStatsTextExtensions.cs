using Utility;

namespace Gameplay.GameMetaStats
{
    /// <summary>
    /// Provides extension methods for generating text summaries from <see cref="GameMetaStatsManager"/> data.
    /// </summary>
    public static class GameMetaStatsTextExtensions
    {
        /// <summary>
        /// Builds a summary text for a win scenario using the provided game statistics.
        /// </summary>
        /// <example>
        /// "You beat the watcher with high hold in 30 minutes and 20 seconds.
        /// It took you 13 rounds and 35 played cards. Nice!"
        /// </example>
        public static string BuildWinSummaryText(this GameMetaStatsManager stats, string endingPhrase)
        {
            if (stats == null)
                return string.Empty;

            string bossName = string.IsNullOrWhiteSpace(stats.BossName)
                ? "the boss"
                : stats.BossName;

            string deckName = string.IsNullOrWhiteSpace(stats.StarterDeckName)
                ? "your deck"
                : stats.StarterDeckName;

            return
                $"You beat '{bossName}' using '{deckName}' as your deck " +
                $"in {stats.GameRuntimeSeconds.ToMinutesSecondsText()}. " +
                $"It took you {stats.RoundsPlayed} rounds and {stats.CardsPlayed} played cards. " +
                $"{endingPhrase}";
        }

        /// <summary>
        /// Builds a summary text for a loss scenario using the provided game statistics.
        /// </summary>
        /// <example>
        /// "You were defeated by the watcher with high hold after 30 minutes and 20 seconds.
        /// You survived 13 rounds and played 35 cards. Better luck next time!"
        /// </example>
        public static string BuildLoseSummaryText(this GameMetaStatsManager stats, string endingPhrase)
        {
            if (stats == null)
                return string.Empty;

            string bossName = string.IsNullOrWhiteSpace(stats.BossName)
                ? "the boss"
                : stats.BossName;

            string deckName = string.IsNullOrWhiteSpace(stats.StarterDeckName)
                ? "your deck"
                : stats.StarterDeckName;

            return
                $"You were defeated by '{bossName}' using '{deckName}' as your deck " +
                $"after {stats.GameRuntimeSeconds.ToMinutesSecondsText()}. " +
                $"You survived {stats.RoundsPlayed} rounds and played {stats.CardsPlayed} cards. " +
                $"{endingPhrase}";
        }
    }
}