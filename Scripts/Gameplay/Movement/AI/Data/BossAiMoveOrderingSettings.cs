using UnityEngine;

namespace Gameplay.Movement.AI.Data
{
    /// <summary>
    /// Designer-tunable settings that influence move ordering heuristics for the boss minimax search.
    /// </summary>
    [CreateAssetMenu(fileName = "BossAiMoveOrderingSettings",
        menuName = "ScriptableObjects/Boss/AI Move Ordering Settings")]
    public sealed class BossAiMoveOrderingSettings : ScriptableObject
    {
        [field: Header("Killer Move Weights")]
        [field: Tooltip(
            "Bonus for moves that previously caused alpha-beta cutoffs at this search depth " +
            "(killer slot #1). Higher values make the boss try these ‘proven strong’ moves first " +
            "during a BossAutoMove phase, which can greatly speed up the search.")]
        [field: SerializeField] public int KillerPrimaryScore { get; private set; } = 1_000_000;

        [field: Tooltip(
            "Bonus for the second stored killer move at this depth (killer slot #2). " +
            "Slightly lower than the primary score so the first killer is still preferred, " +
            "but both are tried early in the move ordering.")]
        [field: SerializeField] public int KillerSecondaryScore { get; private set; } = 900_000;

        [field: Header("Move Type Weights")]
        [field: Tooltip(
            "Bonus given to capture moves when ordering. Higher values make the boss explore " +
            "attacks (removing enemy units) earlier in the minimax search, which can lead to " +
            "more tactical, trade-focused play in the current phase.")]
        [field: SerializeField] public int CaptureScore { get; private set; } = 500_000;

        [field: Header("Move Generator Heuristics")]
        [field: Tooltip(
            "How strongly to trust the local score change provided by the move generator " +
            "(move.HeuristicDelta) when ordering moves. Higher values make moves that look " +
            "locally good (e.g. improving board control, lifetime, threats) get searched earlier.")]
        [field: SerializeField] public float HeuristicDeltaMultiplier { get; private set; } = 100f;

        [field: Tooltip(
            "Weight for how far a move advances a unit towards the opposing back row " +
            "(move.ForwardDelta). Higher values make the AI prefer moves that push units " +
            "forward on the 4x8 board, which can create more pressure and earlier back-row threats.")]
        [field: SerializeField] public int ForwardDeltaMultiplier { get; private set; } = 10;
    }
}