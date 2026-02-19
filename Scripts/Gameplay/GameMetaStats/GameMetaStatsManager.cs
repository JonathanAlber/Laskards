using Gameplay.Boss.Data;
using Gameplay.Cards;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Flow.Gate;
using Gameplay.StarterDecks.Data;
using Systems.Services;
using UnityEngine;

namespace Gameplay.GameMetaStats
{
    /// <summary>
    /// Manages the collection and storage of meta-game statistics.
    /// </summary>
    public class GameMetaStatsManager : GameServiceBehaviour
    {
        /// <summary>
        /// The total amount of real time in seconds this duel has been running.
        /// </summary>
        public float GameRuntimeSeconds { get; private set; }

        /// <summary>
        /// The name of the boss chosen for this duel.
        /// </summary>
        public string BossName { get; private set; }

        /// <summary>
        /// The name of the starter deck chosen for this duel.
        /// </summary>
        public string StarterDeckName { get; private set; }

        /// <summary>
        /// The total number of rounds played in this duel.
        /// </summary>
        public int RoundsPlayed { get; private set; }

        /// <summary>
        /// The total number of cards played by the player during this duel.
        /// </summary>
        public int CardsPlayed { get; private set; }

        private float _startRealtime;

        protected override void Awake()
        {
            base.Awake();

            InitBarrier.OnBecameReady += HandleGateReady;
            GameFlowSystem.OnPhaseStarted += HandlePhaseStarted;
            CardController.OnCardPlayed += HandleCardPlayed;
            GameContextService.OnBossChosen += HandleBossChosen;
            GameContextService.OnStarterDeckChosen += HandleDeckChosen;
            GameFlowSystem.OnGameOver += HandleGameOver;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            InitBarrier.OnBecameReady -= HandleGateReady;
            GameFlowSystem.OnPhaseStarted -= HandlePhaseStarted;
            CardController.OnCardPlayed -= HandleCardPlayed;
            GameContextService.OnBossChosen -= HandleBossChosen;
            GameContextService.OnStarterDeckChosen -= HandleDeckChosen;
            GameFlowSystem.OnGameOver -= HandleGameOver;
        }

        private void HandleGateReady() => _startRealtime = Time.realtimeSinceStartup;

        private void HandleBossChosen(BossData bossData) => BossName = bossData.BossName;

        private void HandleCardPlayed() => CardsPlayed++;

        private void HandleDeckChosen(StarterDeckDefinition starterDeckDefinition)
        {
            StarterDeckName = starterDeckDefinition.DisplayName;
        }

        private void HandlePhaseStarted(GameState gameState)
        {
            if (gameState.CurrentPhase is EGamePhase.BossPrePlay)
                RoundsPlayed++;
        }

        private void HandleGameOver(GameState gameState)
        {
            GameRuntimeSeconds = Time.realtimeSinceStartup - _startRealtime;
        }
    }
}