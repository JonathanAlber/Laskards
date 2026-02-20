using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Units;
using Managers;
using UnityEngine;
using Utility.Logging;

namespace Systems.ObjectPooling.Gameplay
{
    /// <summary>
    /// Pool for <see cref="UnitController"/> instances based on team affiliation.
    /// </summary>
    public class UnitPool : CustomSingleton<UnitPool>
    {
        [Header("Pool Settings")]
        [SerializeField] private UnitController playerUnitPrefab;
        [SerializeField] private UnitController bossUnitPrefab;
        [SerializeField] private Transform playerUnitPoolParent;
        [SerializeField] private Transform bossUnitPoolParent;

        private readonly Dictionary<ETeam, HashSetObjectPool<UnitController>> _pools = new();

        protected override void Awake()
        {
            base.Awake();

            _pools[ETeam.Player] = new HashSetObjectPool<UnitController>(playerUnitPrefab, playerUnitPoolParent,
                ResetInstance);
            _pools[ETeam.Boss] = new HashSetObjectPool<UnitController>(bossUnitPrefab, bossUnitPoolParent,
                ResetInstance);
        }

        /// <summary>
        /// Gets a <see cref="UnitController"/> instance for the specified team.
        /// </summary>
        /// <param name="team">The team affiliation.</param>
        /// <returns><c>true</c> if an instance was retrieved; otherwise, <c>false</c>.</returns>
        public UnitController Get(ETeam team)
        {
            if (_pools.TryGetValue(team, out HashSetObjectPool<UnitController> pool))
                return pool.Get();

            CustomLogger.LogError($"No pool for team {team}", this);
            return null;
        }

        /// <summary>
        /// Releases a <see cref="UnitController"/> instance back to its pool.
        /// </summary>
        /// <param name="unit">The unit instance to release.</param>
        public void Release(UnitController unit)
        {
            ETeam team = unit.Team;
            if (_pools.TryGetValue(team, out HashSetObjectPool<UnitController> pool))
                pool.Release(unit);
            else
                Destroy(unit.gameObject);
        }

        private void ResetInstance(UnitController instance)
        {
            switch (instance.Team)
            {
                case ETeam.Player:
                    instance.transform.SetParent(playerUnitPoolParent);
                    break;
                case ETeam.Boss:
                    instance.transform.SetParent(bossUnitPoolParent);
                    break;
                default:
                    CustomLogger.LogWarning($"Unknown team {instance.Team} for unit instance.", this);
                    break;
            }
        }
    }
}