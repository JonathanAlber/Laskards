using System;
using UnityEngine;

namespace Gameplay.Cards.Effects.Display
{
    /// <summary>
    /// Data class representing effect information for UI display.
    /// </summary>
    [Serializable]
    public class EffectData
    {
        [field: Tooltip("The display name shown in-game.")]
        [field: SerializeField] public string DisplayName { get; private set; }

        [field: Tooltip("Description text shown in tooltips or UI.")]
        [field: TextArea(1,2), SerializeField] public string Description { get; private set; }

        [field: Header("Visuals")]
        [field: Tooltip("The icon used to represent the modifier.")]
        [field: SerializeField] public Sprite Icon { get; private set; }
    }
}