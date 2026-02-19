using UnityEngine;

namespace Gameplay.Movement.AI.Data
{
    /// <summary>
    /// Designer-tunable evaluation settings used by the boss minimax AI.
    /// </summary>
    [CreateAssetMenu(fileName = "BossAiEvaluationSettings", menuName = "ScriptableObjects/Boss/AI Evaluation Settings")]
    public sealed class BossAiEvaluationSettings : ScriptableObject
    {
        [field: Header("Unit Value")]
        [field: Tooltip("How strongly the AI values the effective worth of its own units compared to the player's units.")]
        [field: SerializeField] public float UnitValueWeight { get; private set; } = 1.0f;

        [field: Header("Player Health Impact")]
        [field: Tooltip("How important lowering the player's health is when evaluating a move. Higher = more aggressive AI.")]
        [field: SerializeField] public float PlayerHealthWeight { get; private set; } = 2.0f;

        [field: Header("Board Positioning")]
        [field: Tooltip("How much positional advantage (e.g. forward progress) contributes to scoring for boss units.")]
        [field: SerializeField] public float PositionWeight { get; private set; } = 0.2f;

        [field: Tooltip("How strongly the AI prefers occupying center rows.")]
        [field: SerializeField] public float CenterPositionWeight { get; private set; } = 0.2f;

        [field: Tooltip("Multiplier applied to positional penalties for player units approaching the boss’s side.")]
        [field: SerializeField] public float PlayerPositionPenaltyMultiplier { get; private set; } = 0.5f;

        [field: Header("Lifetime Handling")]
        [field: Tooltip("How severely the AI penalizes boss units with exactly 1 turn of lifetime left.")]
        [field: SerializeField] public float LifetimeRiskWeight { get; private set; } = 0.7f;

        [field: Tooltip("Soft penalty applied to boss units with low remaining lifetime (e.g. 2–3 turns left).")]
        [field: SerializeField] public float LifetimeSoftPenalty { get; private set; } = 0.3f;

        [field: Tooltip("Bonus applied to boss units with infinite lifetime (-1), rewarding long-term value.")]
        [field: SerializeField] public float LifetimeInfiniteBonusWeight { get; private set; } = 0.25f;

        [field: Header("Threat Handling")]
        [field: Tooltip("Penalty applied when player units threaten reaching the boss’s back row (dangerous for the boss).")]
        [field: SerializeField] public float PlayerBackRowThreatWeight { get; private set; } = 1.5f;

        [field: Tooltip("How strongly the AI penalizes boss units that are vulnerable to a pawn spawned on the player's back row.")]
        [field: SerializeField] public float SpawnThreatWeight { get; private set; } = 2.0f;

        [field: Header("Attackable / Invulnerability Handling")]
        [field: Tooltip("Multiplier applied to spawn-threat penalties when the unit CannotBeAttacked. 1.0 = no reduction, 0.0 = full immunity.")]
        [field: SerializeField] public float SpawnThreatUnattackableMultiplier { get; private set; } = 0.25f;

        [field: Tooltip("Multiplier applied per remaining duration turn of an invulnerability effect.")]
        [field: SerializeField] public float SpawnThreatInvulnerabilityDurationMultiplier { get; private set; } = 0.15f;

        [field: Tooltip("Minimum multiplier so penalties never fully disappear accidentally.")]
        [field: SerializeField] public float SpawnThreatMinMultiplier { get; private set; } = 0.1f;

        [field: Header("Tactical Awareness")]
        [field: Tooltip("Penalty applied to boss units that can be captured by the player next move.")]
        [field: SerializeField] public float DangerWeight { get; private set; } = 1.0f;

        [field: Tooltip("Multiplier applied per threatening player unit when calculating danger penalties.")]
        [field: SerializeField] public float DangerAmountMultiplier { get; private set; } = 0.25f;

        [field: Tooltip("Extra multiplier when a boss unit is attacked and not defended by any boss unit.")]
        [field: SerializeField] public float DangerUndefendedMultiplier { get; private set; } = 0.75f;

        [field: Header("Mobility")]
        [field: Tooltip("How important relative mobility (number of legal moves) is between boss and player.")]
        [field: SerializeField] public float MobilityWeight { get; private set; } = 0.1f;

        [field: Tooltip("Multiplier applied to player mobility so high player mobility is penalized more strongly.")]
        [field: SerializeField] public float PlayerMobilityPenaltyMultiplier { get; private set; } = 1.0f;

        [field: Header("Debug")]
        [field: Tooltip("If enabled, the AI will log detailed evaluation information to the console.")]
        [field: SerializeField] public bool EnableDetailLogging { get; private set; }

        [field: Tooltip("Enable detailed per-unit BossUnitValue logging.")]
        [field: SerializeField] public bool EnableBossUnitValueLogging { get; private set; }
    }
}