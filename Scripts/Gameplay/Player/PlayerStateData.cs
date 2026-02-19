using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// Definition of player related data.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStateData", menuName = "ScriptableObjects/Player/Player State Data")]
    public class PlayerStateData : ScriptableObject
    {
        [Tooltip("Maximum health of the player.")]
        [field: SerializeField, Min(0)] public int MaxHealth { get; private set; } = 30;

        [Tooltip("Base energy the player gains at the start of each round.")]
        [field: SerializeField, Min(0)] public int BaseEnergyPerRound { get; private set; } = 1;

        [Tooltip("Number of unit cards the player draws at the start of each turn.")]
        [field: SerializeField, Min(0)] public int BaseCardsToDrawPerTurn { get; private set; } = 2;
    }
}