using System.Collections.Generic;
using Gameplay.Flow.Data;
using UnityEngine;

namespace Gameplay.Flow
{
    /// <summary>
    /// ScriptableObject defining the sequence of game phases for player and boss turns.
    /// </summary>
    [CreateAssetMenu(fileName = "GamePhaseSequence", menuName = "ScriptableObjects/Game Flow/Phase Sequence")]
    public class GamePhaseSequence : ScriptableObject
    {
        [field: Tooltip("Order of phases during the player's turn")]
        [field: SerializeField] public List<EGamePhase> PlayerPhases { get; private set; }

        [field: Tooltip("Order of phases during the boss's turn")]
        [field: SerializeField] public List<EGamePhase> BossPhases { get; private set; }

        [field: Tooltip("Which side takes the first turn")]
        [field: SerializeField] public ETurnOwner StartingTurn { get; private set; } = ETurnOwner.Boss;
    }
}