using Gameplay.Cards.Model;
using Gameplay.Units;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Cards.View
{
    /// <summary>
    /// View for a <see cref="UnitCardModel"/>.
    /// Binds UI elements to the model's data and updates them on changes.
    /// </summary>
    public sealed class UnitCardView : CardView
    {
        [Header("Unit Card View")]
        [Tooltip("Root GameObject for the card visuals that can be toggled on/off.")]
        [SerializeField] private GameObject visualParent;

        [Tooltip("Image component that displays the bird associated with the unit.")]
        [SerializeField] private Image birdImage;

        /// <summary>
        /// Sets the bird sprite based on the unit type.
        /// </summary>
        public void SetBirdSprite(EUnitType unitType)
        {
            Sprite sprite = UnitBirdImageDictionary.Instance.GetSprite(unitType);
            birdImage.sprite = sprite;
        }

        /// <summary>
        /// Sets the visibility of the card visuals.
        /// </summary>
        /// <param name="isVisible">Whether the visuals should be visible.</param>
        public void SetVisible(bool isVisible) => visualParent.SetActive(isVisible);
    }
}