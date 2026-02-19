using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Logging;
using Random = UnityEngine.Random;

namespace Gameplay.Boss.Randomizer
{
    /// <summary>
    /// Runtime instance of a boss pattern definition.
    /// Responsible for selecting entries and tracking state.
    /// </summary>
    public sealed class BossPatternRuntime
    {
        private readonly BossPatternDefinition _definition;
        private readonly int[] _lastEntryIndexPerRow;

        private int _currentRowIndex;

        public BossPatternRuntime(BossPatternDefinition definition)
        {
            _definition = definition;
            if (_definition == null)
            {
                _lastEntryIndexPerRow = Array.Empty<int>();
                CustomLogger.LogWarning($"The definition passed to {nameof(BossPatternRuntime)}" +
                                        " is null.", null);
                return;
            }

            if (_definition.Rows == null)
            {
                _lastEntryIndexPerRow = Array.Empty<int>();
                CustomLogger.LogWarning($"The definition passed to {nameof(BossPatternRuntime)}" +
                                        " has null rows.", null);
                return;
            }

            if (_definition.Rows.Count == 0)
            {
                _lastEntryIndexPerRow = Array.Empty<int>();
                CustomLogger.LogWarning($"The definition passed to {nameof(BossPatternRuntime)}" +
                                        " has no rows.", null);
                return;
            }

            _lastEntryIndexPerRow = new int[_definition.Rows.Count];
            for (int i = 0; i < _lastEntryIndexPerRow.Length; i++)
                _lastEntryIndexPerRow[i] = -1; // -1 = no previous choice
        }

        /// <summary>
        /// Tries to get the next entry from the pattern.
        /// </summary>
        /// <param name="entry">The selected entry, if any.</param>
        /// <returns><c>true</c> if an entry was selected; <c>false</c> otherwise.</returns>
        public bool TryGetNextEntry(out BossPatternEntry entry)
        {
            entry = null;

            if (_currentRowIndex < 0 || _currentRowIndex >= _definition.Rows.Count)
                _currentRowIndex = 0;

            BossPatternRow row = _definition.Rows[_currentRowIndex];
            if (row?.Entries == null)
            {
                CustomLogger.LogWarning($"Row {_currentRowIndex} in boss pattern" +
                                        " definition is null.", null);
                AdvanceRowIndex();
                return false;
            }

            if (row.Entries.Count == 0)
            {
                CustomLogger.LogWarning($"Row {_currentRowIndex} in boss pattern" +
                                        " definition has no entries.", null);
                AdvanceRowIndex();
                return false;
            }

            int lastIndex = _lastEntryIndexPerRow[_currentRowIndex];
            int selected = SelectEntryIndex(row, lastIndex);
            if (selected < 0)
            {
                AdvanceRowIndex();
                return false;
            }

            _lastEntryIndexPerRow[_currentRowIndex] = selected;
            entry = row.Entries[selected];

            AdvanceRowIndex();
            return true;
        }

        /// <summary>
        /// Returns a small random delay between individual cards in an entry.
        /// </summary>
        public float GetRandomDelay()
        {
            float min = Mathf.Max(0f, _definition.MinDelayBetweenCards);
            float max = Mathf.Max(min, _definition.MaxDelayBetweenCards);

            return Random.Range(min, max);
        }

        private static int SelectEntryIndex(BossPatternRow row, int lastIndex)
        {
            List<int> candidateIndices = new();
            List<int> weights = new();

            for (int i = 0; i < row.Entries.Count; i++)
            {
                BossPatternEntry e = row.Entries[i];

                if (e?.Cards == null || e.Cards.Count == 0)
                {
                    CustomLogger.LogWarning($"Entry {i} in boss pattern row is null or has no cards.", null);
                    continue;
                }

                // Avoid repeating same entry if there is more than one
                if (row.Entries.Count > 1 && i == lastIndex)
                    continue;

                candidateIndices.Add(i);
                weights.Add(GetWeight(e.Chance));
            }

            if (candidateIndices.Count == 0)
                return -1;

            int totalWeight = 0;
            foreach (int weight in weights)
                totalWeight += weight;

            int roll = Random.Range(0, totalWeight);
            for (int i = 0; i < candidateIndices.Count; i++)
            {
                int w = weights[i];
                if (roll < w)
                    return candidateIndices[i];

                roll -= w;
            }

            return candidateIndices[^1];
        }

        private static int GetWeight(EBossEntryChance chance)
        {
            switch (chance)
            {
                case EBossEntryChance.Low: return 1;
                case EBossEntryChance.High: return 4;
                case EBossEntryChance.Regular:
                default: return 2;
            }
        }

        private void AdvanceRowIndex() => _currentRowIndex = (_currentRowIndex + 1) % _definition.Rows.Count;
    }
}