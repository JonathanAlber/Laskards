using System;
using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Systems.Services;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Units
{
    /// <summary>
    /// Manages all units in the game, handling their registration, unregistration,
    /// and actions during different game phases.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class UnitManager : GameServiceBehaviour, IPhaseSubscriber
    {
        /// <summary>
        /// Event triggered when a unit dies.
        /// </summary>
        public static event Action<UnitController> OnUnitDied;
        /// <summary>
        /// Event triggered when a player unit dies.
        /// </summary>
        public static event Action<UnitController> OnPlayerUnitDied;

        /// <summary>
        /// Event triggered when a boss unit dies.
        /// </summary>
        public static event Action<UnitController> OnBossUnitDied;

        /// <summary>
        /// Event triggered when a boss unit is spawned.
        /// </summary>
        public static event Action<UnitController> OnBossUnitSpawned;

        [Tooltip("The material to use for fade effects.")]
        [SerializeField] private Material fadeMaterial;

        /// <summary>
        /// Exposes a read-only view of all units.
        /// </summary>
        public IReadOnlyList<UnitController> AllUnits
        {
            get
            {
                List<UnitController> allUnits = new(_playerUnits.Count + _bossUnits.Count);
                allUnits.AddRange(_playerUnits);
                allUnits.AddRange(_bossUnits);
                return allUnits.AsReadOnly();
            }
        }

        /// <summary>
        /// Exposes a read-only view of current player units.
        /// </summary>
        public IReadOnlyList<UnitController> PlayerUnits => _playerUnits.AsReadOnly();

        /// <summary>
        /// Exposes a read-only view of current boss units.
        /// </summary>
        public IReadOnlyList<UnitController> BossUnits => _bossUnits.AsReadOnly();

        public EGamePhase[] SubscribedPhases => new[]
        {
            EGamePhase.PlayerPrePlay,
            EGamePhase.BossPrePlay,
            EGamePhase.BossAutoMove
        };

        private readonly List<UnitController> _playerUnits = new();
        private readonly List<UnitController> _bossUnits = new();

        protected override void Awake()
        {
            base.Awake();

            if (!ServiceLocator.TryGet(out GameFlowSystem flow))
                return;

            EGamePhase[] phases = SubscribedPhases;
            foreach (EGamePhase phase in phases)
                flow.RegisterPhaseSubscriber(phase, this);

            GameFlowSystem.OnGameOver += HandleGameOver;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameFlowSystem.OnGameOver -= HandleGameOver;
        }

        /// <summary>
        /// Registers a unit with the manager, categorizing it by team.
        /// </summary>
        /// <param name="unit">The unit to register.</param>
        public void RegisterUnit(UnitController unit)
        {
            if (unit == null)
            {
                CustomLogger.LogWarning("Attempted to register a null unit.", this);
                return;
            }

            switch (unit.Team)
            {
                case ETeam.Player:
                {
                    _playerUnits.Add(unit);
                    break;
                }
                case ETeam.Boss:
                {
                    _bossUnits.Add(unit);
                    OnBossUnitSpawned?.Invoke(unit);
                    break;
                }
                default:
                {
                    CustomLogger.LogWarning($"Attempted to register unit with unknown team: {unit.Team}", this);
                    break;
                }
            }

            unit.OnUnitDied += UnregisterUnit;
        }

        public void OnPhaseStarted(GameState state, GameFlowSystem flow)
        {
            switch (state.CurrentPhase)
            {
                case EGamePhase.PlayerPrePlay:
                    foreach (UnitController unit in _playerUnits)
                        unit.TickEffects();
                    break;

                case EGamePhase.BossPrePlay:
                    foreach (UnitController unit in _bossUnits)
                        unit.TickEffects();
                    break;
            }

            flow.MarkSubscriberDone(this);
        }

        public void OnPhaseEnded(GameState state) { }

        private void UnregisterUnit(UnitController unit)
        {
            if (unit == null)
            {
                CustomLogger.LogWarning("Attempted to unregister a null unit.", this);
                return;
            }

            bool wasPlayerUnit = _playerUnits.Remove(unit);
            bool wasBossUnit = _bossUnits.Remove(unit);

            if (wasPlayerUnit)
                OnPlayerUnitDied?.Invoke(unit);

            if (wasBossUnit)
                OnBossUnitDied?.Invoke(unit);

            OnUnitDied?.Invoke(unit);
        }

        private void HandleGameOver(GameState gameState)
        {
            foreach (UnitController unit in _playerUnits)
                unit.View.StartFadeOutAnimation();

            foreach (UnitController unit in _bossUnits)
                unit.View.StartFadeOutAnimation();
        }
    }
}