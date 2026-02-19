using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Boss.Randomizer
{
    /// <summary>
    /// Represents a row of random boss pattern entries.
    /// Is used in the <see cref="BossPatternDefinition"/> to define possible plays for the boss.
    /// </summary>
    [Serializable]
    public class BossPatternRow
    {
        [field: Tooltip("Random entries in this row")]
        [field: SerializeField] public List<BossPatternEntry> Entries { get; private set; } = new();
    }
}