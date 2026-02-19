using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Board;
using Gameplay.Boss.Data;
using Gameplay.Boss.Randomizer;
using Gameplay.Boss.TileSetup;
using Gameplay.CardExecution;
using Gameplay.CardExecution.Targeting;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.Cards.Model;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Movement;
using Gameplay.Movement.AI;
using Gameplay.Units;
using Gameplay.Units.Data;
using Systems.Services;
using UnityEngine;
using Utility;
using Utility.Logging;
using VFX;

namespace Gameplay.Boss
{
    /// <summary>
    /// Coordinates boss behavior, connects model and view and interacts with game flow.
    /// </summary>
    public sealed class BossController : GameServiceBehaviour, IPhaseSubscriber, ICardPlayer
    {
        /// <summary>
        /// Event invoked when the boss has executed an opening setup.
        /// </summary>
        public static event Action OnOpeningSetupExecuted;

        [Header("Settings")]
        [Tooltip("How deep the boss should search for unit spawn locations using minimax.")]
        [SerializeField] private int bossUnitSpawnSearchDepth = 3;

        [Header("References")]
        [SerializeField] private BossView bossView;

        public EGamePhase[] SubscribedPhases => new[]
        {
            EGamePhase.BossPlay
        };

        public ETeam Team => ETeam.Boss;

        public BossModel Model { get; private set; }

        private BossData _bossData;
        private BossPlayExecutor _playExecutor;
        private BossCardTargetResolver _resolver;
        private BossPatternRuntime _patternRuntime;
        private Coroutine _playRoutine;
        private Coroutine _backRowSetupRoutine;

        private bool _didNormalSetup;
        private bool _didAggressiveSetup;

        protected override void Awake()
        {
            base.Awake();

            GameContextService.OnBossChosen += Initialize;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            PlayerMoveController.OnUnitEnteredLastRow -= HandleUnitEnteredLastRow;
            GameContextService.OnBossChosen -= Initialize;

            if (Model == null)
                return;

            BossModel.OnAggressive -= HandleAggressiveStateChanged;
            Model.OnDeath -= HandleBossDeath;
        }

        public void OnPhaseStarted(GameState state, GameFlowSystem flow)
        {
            if (state.CurrentTurn != ETurnOwner.Boss)
                return;

            switch (state.CurrentPhase)
            {
                case EGamePhase.BossPlay:
                    DoPlay(flow);
                    break;

                default:
                    return;
            }
        }

        /// <summary>
        /// Applies damage to the boss after a delay based on the boss attack tween data.
        /// </summary>
        public void ApplyDamageWithDelay(int damage, EDamageKind damageKind = EDamageKind.Normal)
        {
            if (!ServiceLocator.TryGet(out ParticleTweenDataProvider provider))
                return;

            float delay;
            switch (damageKind)
            {
                case EDamageKind.Normal:
                {
                    delay = provider.Config.BossAttackTweenData.Delay + provider.Config.BossAttackTweenData.Duration;
                    break;
                }
                case EDamageKind.PlayerEffect:
                {
                    delay = provider.Config.EffectDamageTweenData.Delay + provider.Config.EffectDamageTweenData.Duration;
                    break;
                }
                case EDamageKind.Reflection:
                default:
                {
                    CustomLogger.LogWarning($"Unsupported damage kind for delayed application: {damageKind}." +
                                            " Setting delay to 0.", this);
                    delay = 0;
                    break;
                }
            }

            CoroutineRunner.Instance.RunAfterSeconds(()
                => ApplyDamage(damage), delay);
        }

        public bool CanPlay(CardModel cardModel) => true;

        public void OnPhaseEnded(GameState state) { }

        public void OnCardExecuted(CardController card) { }

        public void OnNewUnitSpawned(UnitController unit) { }

        /// <summary>
        /// Applies damage to the boss and checks for game over condition.
        /// </summary>
        public void ApplyDamage(int amount) => Model.ApplyDamage(amount);

        /// <summary>
        /// Gets the current health of the boss.
        /// </summary>
        /// <param name="amount">The amount of health to set.</param>
        public void SetHealth(int amount) => Model.SetHealth(amount);

        /// <summary>
        /// Returns the current health of the boss.
        /// </summary>
        public int GetHealth() => Model.CurrentHp;

        private static void HandleBossDeath()
        {
            if (!ServiceLocator.TryGet(out GameFlowSystem flow))
                return;

            GameState overState = new(EGamePhase.None, ETurnOwner.Player, true);
            flow.InvokeGameOver(overState);
        }

        private void Initialize(BossData bossData)
        {
            if (bossData == null)
            {
                CustomLogger.LogError("BossData is null. Cannot initialize BossController.", this);
                return;
            }

            _bossData = bossData;

            Model = new BossModel(bossData);

            BossModel.OnAggressive += HandleAggressiveStateChanged;
            Model.OnDeath += HandleBossDeath;

            bossView.Initialize(Model);

            _patternRuntime = new BossPatternRuntime(bossData.DefaultPattern);
            CustomLogger.Log("Boss playing with pattern: " + bossData.DefaultPattern.name, this);

            PlayerMoveController.OnUnitEnteredLastRow += HandleUnitEnteredLastRow;

            if (ServiceLocator.TryGet(out BossAutoMoveController autoMove))
                autoMove.Initialize(_bossData.EvaluationSettings);

            _resolver = new BossCardTargetResolver(bossUnitSpawnSearchDepth, autoMove.Minimax);
            _playExecutor = new BossPlayExecutor(this, _resolver);

            if (!ServiceLocator.TryGet(out GameFlowSystem flow))
                return;

            foreach (EGamePhase phase in SubscribedPhases)
                flow.RegisterPhaseSubscriber(phase, this);
        }

        private void HandleAggressiveStateChanged()
        {
            _patternRuntime = new BossPatternRuntime(_bossData.AggressivePattern);
            CustomLogger.Log("Boss is aggressive now. Switching to aggressive pattern: "
                             + _bossData.AggressivePattern.name, this);
        }

        private void HandleUnitEnteredLastRow(UnitController unit)
        {
            if (unit.Team != ETeam.Player)
                return;

            ApplyDamageWithDelay(unit.Model.GetFinalDamage());
        }

        private void DoPlay(GameFlowSystem flow)
        {
            if (!_didNormalSetup)
            {
                if (_backRowSetupRoutine != null)
                    StopCoroutine(_backRowSetupRoutine);

                _backRowSetupRoutine = StartCoroutine(RunBackRowSetupRoutine(_bossData.OpeningSetup, flow));
                _didNormalSetup = true;
                OnOpeningSetupExecuted?.Invoke();
                return;
            }

            if (Model.IsAggressive && !_didAggressiveSetup)
            {
                if (_backRowSetupRoutine != null)
                    StopCoroutine(_backRowSetupRoutine);

                _backRowSetupRoutine = StartCoroutine(RunBackRowSetupRoutine(_bossData.AggressiveOpeningSetup, flow));
                _didAggressiveSetup = true;
                OnOpeningSetupExecuted?.Invoke();
                return;
            }

            DoPlayLoop(flow);
        }

        private void DoPlayLoop(GameFlowSystem flow)
        {
            if (!_patternRuntime.TryGetNextEntry(out BossPatternEntry entry)
                || !TryBuildModelsForEntry(entry, out List<CardModel> orderedModels))
            {
                CustomLogger.LogWarning("Boss could not get a valid pattern entry or build models for it.", this);
                flow.MarkSubscriberDone(this);
                return;
            }

            if (_playRoutine != null)
                StopCoroutine(_playRoutine);

            _playRoutine = StartCoroutine(PlayPhaseRoutine(orderedModels, flow));
        }

        private IEnumerator PlayPhaseRoutine(List<CardModel> models, GameFlowSystem flow)
        {
            yield return StartCoroutine(_playExecutor.ExecuteWithDelay(models, _patternRuntime.GetRandomDelay));
            flow.MarkSubscriberDone(this);
        }

        private bool TryBuildModelsForEntry(BossPatternEntry entry, out List<CardModel> orderedModels)
        {
            orderedModels = null;

            if (entry == null)
            {
                CustomLogger.LogWarning("Boss pattern entry is null.", this);
                return false;
            }

            if (entry.Cards == null || entry.Cards.Count == 0)
            {
                CustomLogger.LogWarning("Boss pattern entry has no cards.", this);
                return false;
            }

            List<CardDefinition> unitDefs = new();
            List<CardDefinition> actionDefs = new();

            foreach (CardDefinition def in entry.Cards)
            {
                if (def == null)
                {
                    CustomLogger.LogWarning("Boss pattern entry contains a null card definition.", this);
                    continue;
                }

                switch (def)
                {
                    case UnitCardDefinition:
                        unitDefs.Add(def);
                        break;

                    case ActionCardDefinition:
                        actionDefs.Add(def);
                        break;

                    default:
                        CustomLogger.LogWarning("Boss pattern entry contains unsupported " +
                                                $"card type: {def.GetType().Name}", this);
                        break;
                }
            }

            List<CardDefinition> sortedDefs = new();
            sortedDefs.AddRange(unitDefs);
            sortedDefs.AddRange(actionDefs);

            return BossCardFactory.TryCreate(sortedDefs, out orderedModels);
        }

        private IEnumerator RunBackRowSetupRoutine(BossOpeningSetupDefinition setup, GameFlowSystem flow)
        {
            if (setup == null)
            {
                CustomLogger.LogWarning("Boss opening setup is null.", this);
                flow.MarkSubscriberDone(this);
                yield break;
            }

            if (!ServiceLocator.TryGet(out GameBoard board))
            {
                flow.MarkSubscriberDone(this);
                yield break;
            }

            int backRow = board.BoardConfiguration.Rows - 1;
            float delay = Mathf.Max(0f, setup.DelayBetweenMoves);

            for (int col = 0; col < board.BoardConfiguration.Columns; col++)
            {
                BossOpeningTileSetup entry = setup.BackRowEntries[col];
                Tile tile = board.GetTile(backRow, col);

                if (tile == null)
                {
                    CustomLogger.LogWarning("Back row tile [" + backRow + "," + col + "] is null.", this);
                    continue;
                }

                if (entry.UnitCard == null)
                    continue;

                yield return new WaitForSeconds(delay);

                if (tile.IsOccupied())
                {
                    tile.ClearEffects();
                    if (tile.OccupyingUnit != null)
                        tile.OccupyingUnit.Die();
                }

                BossSetupTargetResolver forcedResolver = new(tile);

                if (BossCardFactory.TryCreate(entry.UnitCard, out CardModel unitModel)
                    && unitModel is UnitCardModel unitCardModel)
                {
                    UnitCardExecutor.TryExecute(unitCardModel, this, forcedResolver, out _, out _);
                }

                foreach (ActionCardDefinition act in entry.BuffActions)
                {
                    if (act == null || act.Modifier.ActionCategory != EActionCategory.Buff)
                        continue;

                    if (BossCardFactory.TryCreate(act, out CardModel model) && model is ActionCardModel actionModel)
                        ActionCardExecutor.TryExecute(actionModel, this, forcedResolver, out _);
                }
            }

            flow.MarkSubscriberDone(this);
        }
    }
}