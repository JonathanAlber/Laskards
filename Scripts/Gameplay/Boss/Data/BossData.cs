using Gameplay.Boss.Randomizer;
using Gameplay.Boss.TileSetup;
using Gameplay.Movement.AI.Data;
using UnityEngine;

namespace Gameplay.Boss.Data
{
    /// <summary>
    /// Definition of boss related data.
    /// </summary>
    [CreateAssetMenu(fileName = "BossData", menuName = "ScriptableObjects/Boss/Boss Data")]
    public class BossData : ScriptableObject
    {
        [field: Header("Configuration")]
        [field: Tooltip("Maximum health of the boss.")]
        [field: SerializeField, Min(0)] public int Health { get; private set; } = 20;

        [field: Tooltip("Threshold (0-1) of health percentage below which the boss " +
                        "becomes aggressive and switches its patterns.")]
        [field: SerializeField, Range(0f, 1f)] public float AggressiveHealthThreshold { get; private set; } = 0.5f;

        [field: Header("UI Information")]
        [field: Tooltip("The display name of the boss.")]
        [field: SerializeField] public string BossName { get; private set; }

        [field: Tooltip("A brief description of the boss's playstyle.")]
        [field: SerializeField] public string PlaystyleDescription { get; private set; }

        [field: Tooltip("The bosses estimated difficulty level.")]
        [field: SerializeField] public EBossDifficulty BossDifficulty { get; private set; } = EBossDifficulty.Medium;

        [field: Tooltip("A detailed description of the boss.")]
        [field: SerializeField, TextArea(1,2)] public string Description { get; private set; }

        [field: Tooltip("The image used to represent the boss in the UI.")]
        [field: SerializeField] public Sprite BossImage { get; set; }

        [field: Header("Opening Setup")]
        [field: Tooltip("The initial setup of the boss's back row on the board.")]
        [field: SerializeField] public BossOpeningSetupDefinition OpeningSetup { get; private set; }

        [field: Tooltip("The setup of the boss's back row on the board when it becomes aggressive.")]
        [field: SerializeField] public BossOpeningSetupDefinition AggressiveOpeningSetup { get; private set; }

        [field: Header("Card Play Patterns")]
        [field: Tooltip("The default card play pattern for this boss.")]
        [field: SerializeField] public BossPatternDefinition DefaultPattern { get; private set; }

        [field: Tooltip("The card play pattern for this boss, when its health is below a certain threshold.")]
        [field: SerializeField] public BossPatternDefinition AggressivePattern { get; private set; }

        [field: Header("Move Evaluation Settings")]
        [field: Tooltip("The evaluation settings used by the boss AI to evaluate moves.")]
        [field: SerializeField] public BossAiEvaluationSettings EvaluationSettings { get; private set; }
    }
}