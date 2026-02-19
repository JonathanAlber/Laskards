using UnityEngine;

namespace Gameplay.Units.Worth.Data
{
    /// <summary>
    /// Configuration for how different unit stats contribute to their overall worth.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitWorthWeights", menuName = "ScriptableObjects/Units/Unit Worth Weights")]
    public class UnitWorthWeights : ScriptableObject
    {
        [field: Header("Weights")]
        [field: Tooltip("How much weight the unit's damage contributes to its overall worth.")]
        [field: SerializeField] public float DamageWeight { get; private set; } = 0.3f;

        [field: Tooltip("How much weight the unit's current health contributes to its overall worth.")]
        [field: SerializeField] public float CurrentHealthWeight { get; private set; } = 0.2f;

        [field: Tooltip("How much weight the unit's left lifetime contributes to its overall worth.")]
        [field: SerializeField] public float LifetimeWeight { get; private set; } = 0.15f;

        [field: Tooltip("How much weight the unit's base worth contributes to its overall worth.")]
        [field: SerializeField] public float WorthWeight { get; private set; } = 0.15f;

        [field: Tooltip("How much weight the unit's remaining moves contributes to its overall worth.")]
        [field: SerializeField] public float MovesLeftWeight { get; private set; } = 0.2f;

        [field: Header("Bonuses")]
        [field: Tooltip("How much weight is added if the unit can't be attacked.")]
        [field: SerializeField] public float CantBeAttackedBonus { get; private set; } = 1f;

        [field: Tooltip("How much bonus weight is added for units with infinite lifetime.")]
        [field: SerializeField] public float InfiniteLifetimeBonus  { get; private set; } = 1f;

        [field: Header("Duration Weights")]
        [field: Tooltip("Scaling applied to temporary stat gains. Example: 1 = full value, 0.5 = half value.")]
        [field: SerializeField]
        public float TemporaryEffectBaseMultiplier { get; private set; } = 1f;

        [field: Tooltip("Multiplier per remaining duration turn (e.g. 0.5 means each extra turn adds 50% value).")]
        [field: SerializeField]
        public float TemporaryEffectPerTurnBonus { get; private set; } = 0.5f;

        [field: Tooltip("Minimum multiplier applied to expiring effects.")]
        [field: SerializeField]
        public float TemporaryEffectMinMultiplier { get; private set; } = 0.25f;
    }
}