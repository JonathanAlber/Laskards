using System;
using System.Collections.Generic;
using Gameplay.Cards.Data;
using Gameplay.Cards.Data.Properties;
using UnityEngine;

namespace Gameplay.Boss.Randomizer
{
    /// <summary>
    /// Represents a single entry in a boss pattern row.
    /// An entry defines a set of cards that will be played together and their chance to be selected.
    /// </summary>
    [Serializable]
    public class BossPatternEntry
    {
        [field: Tooltip("Cards that will be played together when this entry is chosen.")]
        [field: SerializeField, BossCard] public List<CardDefinition> Cards { get; private set; } = new();

        [field: Tooltip("Relative chance for this entry to be picked when its row is active.")]
        [field: SerializeField] public EBossEntryChance Chance { get; private set; } = EBossEntryChance.Regular;

        [field: Tooltip("Marks this entry as a 'special move' for visual feedback only.")]
        [field: SerializeField] public bool IsSpecialMove { get; private set; }
    }
}