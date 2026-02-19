using Utility.Logging;

namespace Gameplay.Cards.Data
{
    /// <summary>
    /// Utility class for working with <see cref="EActionCategory"/>.
    /// </summary>
    public static class ActionCategoryUtility
    {
        /// <summary>
        /// Converts an action category to a readable string.
        /// </summary>
        /// <param name="actionCategory">The action category to convert.</param>
        /// <returns>A readable string representing the action category.</returns>
        public static string GetActionCategoryName(EActionCategory actionCategory)
        {
            switch (actionCategory)
            {
                case EActionCategory.Player:
                {
                    return "Player";
                }
                case EActionCategory.Buff:
                {
                    return "Buff";
                }
                case EActionCategory.Tile:
                {
                    return "Tile";
                }
                case EActionCategory.Unit:
                {
                    return "non-Raven Unit";
                }
                case EActionCategory.Resource:
                {
                    return "Resource";
                }
                case EActionCategory.None:
                default:
                {
                    CustomLogger.LogWarning($"Encountered unsupported action category {actionCategory}.", null);
                    return "Unknown";
                }
            }
        }
    }
}