using System;
using Attributes;
using Gameplay.Boss.Data;
using Gameplay.Flow.Data;
using Gameplay.StarterDecks.Data;
using Systems.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.Flow
{
    /// <summary>
    /// Manages the current game context, including selected boss and starter deck.
    /// </summary>
    public class GameContextService : GameServiceBehaviour
    {
        /// <summary>
        /// Event invoked when a boss is chosen.
        /// </summary>
        public static event Action<BossData> OnBossChosen;

        /// <summary>
        /// Event invoked when a starter deck is chosen.
        /// </summary>
        public static event Action<StarterDeckDefinition> OnStarterDeckChosen;

        [Tooltip("When these scenes are loaded, the game context will reset," +
                 " if no instruction to the contrary is given.")]
        [SceneName, SerializeField] private string[] scenesToResetAt;

        /// <summary>
        /// The currently selected boss for the run.
        /// </summary>
        public BossData SelectedBoss { get; private set; }

        /// <summary>
        /// The currently selected starter deck for the run.
        /// </summary>
        public StarterDeckDefinition SelectedStarterDeck { get; private set; }

        private ERunStartMode _runStartMode;

        protected override void Awake()
        {
            base.Awake();

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode _)
        {
            if (_runStartMode != ERunStartMode.NewRun)
            {
                // Reset mode for next scene load
                _runStartMode = ERunStartMode.NewRun;
                return;
            }

            foreach (string sceneName in scenesToResetAt)
            {
                if (!scene.name.Equals(sceneName, StringComparison.OrdinalIgnoreCase))
                    continue;

                PrepareRun(ERunStartMode.NewRun);
                break;
            }
        }

        /// <summary>
        /// Prepare for a new run or change in selections.
        /// </summary>
        /// <param name="mode">The mode indicating what to clear.</param>
        public void PrepareRun(ERunStartMode mode)
        {
            _runStartMode = mode;
            switch (_runStartMode)
            {
                case ERunStartMode.NewRun:
                    ClearBoss();
                    ClearDeck();
                    break;
                case ERunStartMode.ChangeBoss:
                    ClearBoss();
                    break;
                case ERunStartMode.ChangeDeck:
                    ClearDeck();
                    break;
                case ERunStartMode.Retry:
                    // Keep selections
                    break;
            }
        }

        /// <summary>
        /// Choose the boss for the current run.
        /// </summary>
        public void ChooseBoss(BossData boss)
        {
            SelectedBoss = boss;
            OnBossChosen?.Invoke(boss);
        }

        /// <summary>
        /// Choose the starter deck for the current run.
        /// </summary>
        public void ChooseStarterDeck(StarterDeckDefinition deck)
        {
            SelectedStarterDeck = deck;
            OnStarterDeckChosen?.Invoke(deck);
        }

        /// <summary>
        /// If selections are already present, re-emit them so gates/menus depending on events still work.
        /// </summary>
        public void RaiseCachedSelectionsIfAny()
        {
            if (SelectedBoss != null)
                OnBossChosen?.Invoke(SelectedBoss);

            if (SelectedStarterDeck != null)
                OnStarterDeckChosen?.Invoke(SelectedStarterDeck);
        }

        private void ClearBoss() => SelectedBoss = null;

        private void ClearDeck() => SelectedStarterDeck = null;
    }
}