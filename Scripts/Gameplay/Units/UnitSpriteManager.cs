using Framework;
using Gameplay.CardExecution;
using UnityEngine;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Units
{
    /// <summary>
    /// Manager for unit sprites and colors based on unit type and team.
    /// </summary>
    public class UnitSpriteManager : CustomSingleton<UnitSpriteManager>
    {
        [Header("Unit Icons")]
        [SerializeField] private SerializableDictionary<EUnitType, Sprite> playerUnitToIconMap;
        [SerializeField] private SerializableDictionary<EUnitType, Sprite> bossUnitToIconMap;

        /// <summary>
        /// Gets the icon associated with the specified unit type and team.
        /// </summary>
        public Sprite GetUnitIcon(EUnitType unitType, ETeam team)
        {
            switch (team)
            {
                case ETeam.Player:
                {
                    if (!playerUnitToIconMap.ContainsKey(unitType))
                    {
                        CustomLogger.LogWarning($"No icon mapping for player unit type {unitType}", this);
                        return null;
                    }

                    return playerUnitToIconMap[unitType];
                }
                case ETeam.Boss:
                {
                    if (!bossUnitToIconMap.ContainsKey(unitType))
                    {
                        CustomLogger.LogWarning($"No icon mapping for boss unit type {unitType}", this);
                        return null;
                    }

                    return bossUnitToIconMap[unitType];
                }
                default:
                {
                    CustomLogger.LogWarning($"No icon mapping for team {team}", this);
                    return null;
                }
            }
        }
    }
}