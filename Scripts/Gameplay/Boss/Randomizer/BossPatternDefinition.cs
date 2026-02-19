using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Boss.Randomizer
{
    /// <summary>
    /// Definition of a boss pattern used to randomize boss behavior.
    /// It defines which cards the boss can play and the timing between them.
    /// </summary>
    [CreateAssetMenu(fileName = "BossPattern", menuName = "ScriptableObjects/Boss/Boss Pattern")]
    public class BossPatternDefinition : ScriptableObject
    {
        [field: Header("Timing")]
        [field: Tooltip("Minimum delay in seconds between individual cards in the same entry.")]
        [field: SerializeField] public float MinDelayBetweenCards { get; private set; } = 0.35f;

        [field: Tooltip("Maximum delay in seconds between individual cards in the same entry.")]
        [field: SerializeField] public float MaxDelayBetweenCards { get; private set; } = 0.75f;

        [field: Header("Rows")]
        [field: Tooltip("Rows of random entries. Row 0 is the first row used.")]
        [field: SerializeField] public List<BossPatternRow> Rows { get; private set; } = new();
    }
}