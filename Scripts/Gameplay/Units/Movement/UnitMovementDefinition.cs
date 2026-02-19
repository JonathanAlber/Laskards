using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utility.Logging;

namespace Gameplay.Units.Movement
{
    /// <summary>
    /// Immutable collection of movement rules that fully describes how a unit may move.
    /// This is pure game data and can be shared between player and boss logic.
    /// </summary>
    public sealed class UnitMovementDefinition
    {
        /// <summary>
        /// All directional movement rules for this unit.
        /// </summary>
        public IReadOnlyList<UnitMovementRule> Rules { get; }

        public UnitMovementDefinition(IReadOnlyList<UnitMovementRule> rules)
        {
            if (rules == null)
            {
                CustomLogger.LogError("Passed null rules to constructor.", null);
                return;
            }

            List<UnitMovementRule> copy = new(rules.Count);

            foreach (UnitMovementRule rule in rules)
            {
                if (rule == null)
                {
                    CustomLogger.LogError("Encountered null rule in movement definition; skipping.", null);
                    continue;
                }

                copy.Add(rule);
            }

            Rules = new ReadOnlyCollection<UnitMovementRule>(copy);
        }
    }
}