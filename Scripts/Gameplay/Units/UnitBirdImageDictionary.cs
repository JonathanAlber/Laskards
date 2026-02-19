using Framework;
using UnityEngine;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Units
{
    /// <summary>
    /// Dictionary mapping unit types to their corresponding bird image sprites.
    /// </summary>
    public class UnitBirdImageDictionary : CustomSingleton<UnitBirdImageDictionary>
    {
        [Tooltip("Mapping of unit types to their corresponding bird image sprites.")]
        [SerializeField] private SerializableDictionary<EUnitType, Sprite> sprites;

        /// <summary>
        /// Gets the bird image sprite for the specified unit type.
        /// </summary>
        /// <param name="unitType">The unit type.</param>
        /// <returns></returns>
        public Sprite GetSprite(EUnitType unitType)
        {
            if (sprites.TryGetValue(unitType, out Sprite sprite))
                return sprite;

            CustomLogger.LogWarning($"No bird image found for unit type {unitType}.", this);
            return null;
        }
    }
}