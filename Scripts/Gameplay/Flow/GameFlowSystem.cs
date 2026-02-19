using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Flow.Data;
using Gameplay.Flow.Gate;
using Systems.Services;
using UnityEngine;
using Utility;

namespace Gameplay.Flow
{
    /// <summary>
    /// Manages the flow of the game through its various phases and turns.
    /// </summary>
    [DefaultExecutionOrder(-15)]
    public class GameFlowSystem : GameServiceBehaviour
    {
        /// <summary>
        /// Event triggered when a game phase starts.
        /// </summary>
        public static event Action<GameState> OnPhaseStarted;

        /// <summary>
        /// Event triggered when a game phase ends.
        /// </summary>
        public static event Action<GameState> OnPhaseEnded;

        /// <summary>
        /// Event triggered when the game is over.
        /// </summary>
        public static event Action<GameState> OnGameOver;

        /// <summary>
        /// The current phase of the game.
        /// </summary>
        public static EGamePhase CurrentPhase;

        [Header("References")]
        [SerializeField] private GamePhaseSequence phaseSequence;

        [Header("Settings")]
        [Tooltip("A short delay before starting the first turn after initialization" +
                 " so players can adjust to the game starting.")]
        [SerializeField] private float delayBeforeStartingFirstTurn = 1f;

        private readonly Dictionary<EGamePhase, HashSet<IPhaseSubscriber>> _phaseSubscribers = new();
        private readonly HashSet<IPhaseSubscriber> _activeSubscribers = new();

        private Coroutine _beginTurnDelay;
        private Coroutine _runTurnPhasesCoroutine;

        private GameState _state;
        private EGamePhase _phaseOverrideTarget;

        private bool _phaseOverrideRequested;
        private bool _phaseStartEventAlreadyRaisedForOverride;

        protected override void Awake()
        {
            base.Awake();

            _state = new GameState(EGamePhase.None, phaseSequence.StartingTurn, false);

            if (ServiceLocator.TryGet(out InitGateHub hub) && hub?.Barrier != null)
                InitBarrier.OnBecameReady += OnGateReady;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_runTurnPhasesCoroutine != null)
                StopCoroutine(_runTurnPhasesCoroutine);

            if (_beginTurnDelay != null)
                StopCoroutine(_beginTurnDelay);

            InitBarrier.OnBecameReady -= OnGateReady;
        }

        /// <summary>
        /// Registers a subscriber for a specific game phase.
        /// </summary>
        /// <param name="phase">The game phase to subscribe to.</param>
        /// <param name="subscriber">The subscriber to register.</param>
        public void RegisterPhaseSubscriber(EGamePhase phase, IPhaseSubscriber subscriber)
        {
            if (!_phaseSubscribers.TryGetValue(phase, out HashSet<IPhaseSubscriber> list))
            {
                list = new HashSet<IPhaseSubscriber>();
                _phaseSubscribers[phase] = list;
            }

            list.Add(subscriber);
        }

        /// <summary>
        /// Deregisters a subscriber from a specific game phase.
        /// </summary>
        /// <param name="phase">The game phase to unsubscribe from.</param>
        /// <param name="subscriber">The subscriber to deregister.</param>
        public void DeregisterPhaseSubscriber(EGamePhase phase, IPhaseSubscriber subscriber)
        {
            if (_phaseSubscribers.TryGetValue(phase, out HashSet<IPhaseSubscriber> list))
                list.Remove(subscriber);
        }

        /// <summary>
        /// Marks a subscriber as done for the current phase.
        /// </summary>
        /// <param name="subscriber">The subscriber to mark as done.</param>
        public void MarkSubscriberDone(IPhaseSubscriber subscriber) => _activeSubscribers.Remove(subscriber);

        /// <summary>
        /// Invokes the game over event with the specified game state.
        /// </summary>
        /// <param name="gameState">The final game state.</param>
        public void InvokeGameOver(GameState gameState)
        {
            _state = gameState.WithGameOver();
            OnGameOver?.Invoke(_state);
        }

        /// <summary>
        /// Forces a change to a specific game phase, overriding the normal flow.
        /// </summary>
        /// <param name="newPhase">The game phase to switch to.</param>
        public void ForcePhaseChange(EGamePhase newPhase)
        {
            _phaseOverrideRequested = true;
            _phaseOverrideTarget = newPhase;

            _activeSubscribers.Clear();

            OnPhaseEnded?.Invoke(_state);

            CurrentPhase = newPhase;
            _state = _state.WithPhase(CurrentPhase);

            _phaseStartEventAlreadyRaisedForOverride = true;

            OnPhaseStarted?.Invoke(_state);
        }

        private void BeginTurn()
        {
            List<EGamePhase> turnPhases = _state.CurrentTurn == ETurnOwner.Player
                ? phaseSequence.PlayerPhases
                : phaseSequence.BossPhases;

            if (_runTurnPhasesCoroutine != null)
                StopCoroutine(_runTurnPhasesCoroutine);

            _runTurnPhasesCoroutine = StartCoroutine(RunTurnPhases(turnPhases));
        }

        private IEnumerator RunTurnPhases(List<EGamePhase> turnPhases)
        {
            int i = 0;

            while (i < turnPhases.Count)
            {
                if (_phaseOverrideRequested)
                {
                    int idx = turnPhases.IndexOf(_phaseOverrideTarget);
                    if (idx >= 0)
                        i = idx;

                    _phaseOverrideRequested = false;
                }

                EGamePhase phase = turnPhases[i];

                CurrentPhase = phase;
                _state = _state.WithPhase(CurrentPhase);

                if (!_phaseStartEventAlreadyRaisedForOverride)
                    OnPhaseStarted?.Invoke(_state);
                else
                    _phaseStartEventAlreadyRaisedForOverride = false;

                yield return RunPhase(phase);

                if (_state.IsGameOver)
                    yield break;

                OnPhaseEnded?.Invoke(_state);

                i++;
            }

            _state = _state.WithTurn(_state.CurrentTurn == ETurnOwner.Player ? ETurnOwner.Boss : ETurnOwner.Player);
            BeginTurn();
        }

        private IEnumerator RunPhase(EGamePhase phase)
        {
            _activeSubscribers.Clear();

            if (_phaseSubscribers.TryGetValue(phase, out HashSet<IPhaseSubscriber> subscribers))
            {
                foreach (IPhaseSubscriber sub in subscribers)
                {
                    _activeSubscribers.Add(sub);
                    sub.OnPhaseStarted(_state, this);
                }
            }

            while (_activeSubscribers.Count > 0)
                yield return null;

            if (!_phaseSubscribers.TryGetValue(phase, out HashSet<IPhaseSubscriber> endedSubscribers))
                yield break;

            foreach (IPhaseSubscriber sub in endedSubscribers)
                sub.OnPhaseEnded(_state);
        }
        private void OnGateReady()
        {
            InitBarrier.OnBecameReady -= OnGateReady;
            _beginTurnDelay = CoroutineRunner.Instance.RunAfterSeconds(BeginTurn, delayBeforeStartingFirstTurn);
        }
    }
}