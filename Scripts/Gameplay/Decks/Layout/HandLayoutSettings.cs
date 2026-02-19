using UnityEngine;

namespace Gameplay.Decks.Layout
{
    /// <summary>
    /// Tunable data for how the hand is visually fanned:
    /// arc size, radius, tilt, spacing, and resting scale.
    /// </summary>
    [CreateAssetMenu(fileName = "HandLayoutSettings", menuName = "ScriptableObjects/Decks/Layout/Hand Layout Settings")]
    public sealed class HandLayoutSettings : ScriptableObject
    {
        [field: Header("Arc")]
        [field: Tooltip("Distance from the hand pivot to the arc that cards sit on. Negative bends the arc toward the camera.")]
        [field: SerializeField] public float Radius { get; private set; } = -300f;

        [field: Tooltip("Max arc angle (in degrees) across the full hand when it's wide.")]
        [field: SerializeField] public float ArcDegrees { get; private set; } = 120f;

        [field: Tooltip("Clamp for per-card Z rotation so cards don't tilt too hard.")]
        [field: SerializeField] public float MaxAnglePerCard { get; private set; } = 25f;

        [field: Header("Small-Hand Spread")]
        [field: Tooltip("Arc to use when the hand is tiny (keeps 1–3 cards near center).")]
        [field: SerializeField] public float MinArcDegrees { get; private set; } = 13f;

        [field: Tooltip("Card count at which we consider the hand 'full width' and use ArcDegrees.")]
        [field: SerializeField] public int FullSpreadAtCount { get; private set; } = 6;

        [field: Header("Spacing")]
        [field: Tooltip("Extra horizontal offset per index to avoid perfect overlap.")]
        [field: SerializeField] public float CardSpacing { get; private set; } = 4f;

        [field: Header("Scale")]
        [field: Tooltip("Resting scale for cards while they live in the hand.")]
        [field: SerializeField] public Vector3 TargetScale { get; private set; } = Vector3.one;
    }
}