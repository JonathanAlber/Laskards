using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Boss.Data
{
    /// <summary>
    /// ScriptableObject representing a collection of bosses available in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "BossDataCollection", menuName = "ScriptableObjects/Boss/Boss Data Collection")]
    public class BossDataCollection : ScriptableObject
    {
        [field: SerializeField, Tooltip("Bosses available for selection in-game.")]
        public List<BossData> BossData { get; private set; } = new();
    }
}