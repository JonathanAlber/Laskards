using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gameplay.Board;
using Gameplay.Boss;
using Gameplay.CardExecution;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Movement.AI.Data;
using Gameplay.Units;
using Gameplay.Units.Movement;
using Gameplay.Units.Worth;
using Systems.Services;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Controls boss unit movement during its movement phase.
    /// Uses depth-limited minimax search with alpha-beta pruning to select moves.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public sealed class BossAutoMoveController : GameServiceBehaviour, IPhaseSubscriber
    {
        /// <summary>
        /// Fired when a move has been successfully performed.
        /// </summary>
        public static event Action<UnitController, Tile, Tile> OnMovePerformed;

        /// <summary>
        /// Fired when the boss starts thinking about its moves.
        /// </summary>
        public static event Action OnBossThinkingStarted;

        /// <summary>
        /// Fired when the boss has finished thinking about its moves.
        /// </summary>
        public static event Action OnBossThinkingEnded;

        /// <summary>
        /// Fired when a unit enters the last row of the opponent.
        /// </summary>
        public static event Action<UnitController> OnUnitEnteredLastRow;

        [Header("Minimax Settings")]
        [SerializeField] private int searchDepth = 4;

        [Header("Move Timing")]
        [SerializeField] private float minMoveDelay = 0.1f;
        [SerializeField] private float maxMoveDelay = 0.3f;

        [field: Header("References")]
        [field: SerializeField] public BossAiMoveOrderingSettings MoveOrderingSettings { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool useFixedMoveDelay;
        [SerializeField] private float fixedMoveDelay = 0.2f;

        public EGamePhase[] SubscribedPhases => new[] { EGamePhase.BossAutoMove };

        public BossMinimaxSearch Minimax { get; private set; }

        private GameBoard _board;
        private Coroutine _moveRoutine;
        private BossAiEvaluator _evaluator;
        private UnitTweenController _unitTween;
        private AiMoveGenerator _moveGenerator;
        private AiUnitValueCalculator _unitValueCalculator;

        private bool _shouldSkipTurn;

        protected override void Awake()
        {
            base.Awake();

            BossController.OnOpeningSetupExecuted += HandleOpeningSetupExecuted;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            BossController.OnOpeningSetupExecuted -= HandleOpeningSetupExecuted;
        }

        /// <summary>
        /// Initializes the boss AI move controller with the given evaluation settings.
        /// </summary>
        public void Initialize(BossAiEvaluationSettings evaluationSettings)
        {
            if (!ServiceLocator.TryGet(out GameFlowSystem flow))
                return;

            foreach (EGamePhase phase in SubscribedPhases)
                flow.RegisterPhaseSubscriber(phase, this);

            BossAiStateBuilder builder = BossAiStateBuilder.TryCreateFromServices();
            if (builder == null)
            {
                CustomLogger.LogError("Could not create AI state builder from services.", this);
                return;
            }

            ServiceLocator.TryGet(out _board);
            ServiceLocator.TryGet(out _unitTween);

            ServiceLocator.TryGet(out UnitWorthCalculator worthCalculator);
            _unitValueCalculator = new AiUnitValueCalculator(worthCalculator.WorthWeights);

            _evaluator = new BossAiEvaluator(evaluationSettings, _unitValueCalculator);
            _moveGenerator = new AiMoveGenerator(_unitValueCalculator);
            Minimax = new BossMinimaxSearch(_evaluator, _moveGenerator, MoveOrderingSettings);
        }

        public void OnPhaseStarted(GameState state, GameFlowSystem flow)
        {
            if (state.CurrentPhase != EGamePhase.BossAutoMove)
            {
                flow.MarkSubscriberDone(this);
                return;
            }

            if (_shouldSkipTurn)
            {
                _shouldSkipTurn = false;
                flow.MarkSubscriberDone(this);
                return;
            }

            if (_evaluator == null)
            {
                CustomLogger.LogWarning("Evaluator is null.", this);
                flow.MarkSubscriberDone(this);
                return;
            }

            if (Minimax == null)
            {
                CustomLogger.LogWarning("Minimax search is null.", this);
                flow.MarkSubscriberDone(this);
                return;
            }

            BossAiStateBuilder builder = BossAiStateBuilder.TryCreateFromServices();
            if (builder == null)
            {
                flow.MarkSubscriberDone(this);
                return;
            }

            AiGameState aiState = builder.Build(out Dictionary<int, UnitController> idMap);

            if (_moveRoutine != null)
                StopCoroutine(_moveRoutine);

            _moveRoutine = StartCoroutine(DoSequentialMinimaxMoves(aiState, idMap, flow));
        }

        public void OnPhaseEnded(GameState state)
        {
            if (state.CurrentPhase != EGamePhase.BossAutoMove)
                return;

            OnBossThinkingEnded?.Invoke();

            if (_moveRoutine == null)
                return;

            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        private static int CountBossUnitsThatCanMove(Dictionary<int, UnitController> map)
        {
            int count = 0;

            foreach (KeyValuePair<int, UnitController> kv in map)
            {
                UnitController u = kv.Value;

                if (u == null)
                    continue;

                if (u.Model == null)
                    continue;

                if (!u.Model.IsAlive)
                    continue;

                if (u.Team != ETeam.Boss)
                    continue;

                if (u.Model.RemainingMoves <= 0)
                    continue;

                count++;
            }

            return count;
        }

        private IEnumerator DoSequentialMinimaxMoves(AiGameState aiState, Dictionary<int, UnitController> map,
            GameFlowSystem flow)
        {
            while (true)
            {
                // If no boss units can move anymore, stop the phase
                if (CountBossUnitsThatCanMove(map) == 0)
                    break;

                AiMove chosenMove = AiMove.CreatePass();

                AiGameState stateCopy = aiState;

                float searchStart = Time.realtimeSinceStartup;

                OnBossThinkingStarted?.Invoke();

                Task<bool> searchTask = Task.Run(() => Minimax.TryFindBestMove(stateCopy, searchDepth, out chosenMove));
                while (!searchTask.IsCompleted)
                    yield return null;

                float searchTime = Time.realtimeSinceStartup - searchStart;

                if (!searchTask.Result)
                    break;

                // Minimax decided that ending the phase (pass) is best.
                if (chosenMove.IsPass)
                    break;

                float delay = useFixedMoveDelay
                    ? fixedMoveDelay
                    : UnityEngine.Random.Range(minMoveDelay, maxMoveDelay);

                float remainingDelay = Mathf.Max(0f, delay - searchTime);
                if (remainingDelay > 0f)
                    yield return new WaitForSeconds(remainingDelay);

                OnBossThinkingEnded?.Invoke();

                bool moveCompleted = false;

                if (!ExecuteMove(chosenMove, map, () => moveCompleted = true))
                    break;

                // Wait for tween to finish
                yield return new WaitUntil(() => moveCompleted);

                BossAiStateBuilder builder = BossAiStateBuilder.TryCreateFromServices();
                if (builder == null)
                    break;

                aiState = builder.Build(out map);
            }

            OnBossThinkingEnded?.Invoke();
            _moveRoutine = null;
            flow.MarkSubscriberDone(this);
        }

        private bool ExecuteMove(AiMove move, Dictionary<int, UnitController> map, Action onVisualComplete)
        {
            if (move.IsPass)
                return false;

            if (!map.TryGetValue(move.UnitId, out UnitController unit))
            {
                CustomLogger.LogWarning($"Could not find unit with ID {move.UnitId} to execute move.", this);
                return false;
            }

            if (unit == null)
            {
                CustomLogger.LogWarning($"Null unit for ID {move.UnitId} to execute move.", this);
                return false;
            }

            if (unit.Model == null)
            {
                CustomLogger.LogWarning($"Unit with ID {move.UnitId} has no model.", this);
                return false;
            }

            if (!unit.Model.IsAlive)
            {
                CustomLogger.LogWarning($"Unit with ID {move.UnitId} is not alive.", this);
                return false;
            }

            if (_board == null)
            {
                CustomLogger.LogWarning("Game board is null.", this);
                return false;
            }

            Tile target = _board.GetTile(move.ToRow, move.ToColumn);
            if (target == null)
            {
                CustomLogger.LogWarning($"Could not find target tile at ({move.ToRow}, {move.ToColumn}).", this);
                return false;
            }

            Tile origin = unit.CurrentTile;
            if (origin == null)
            {
                CustomLogger.LogWarning("Unit has no origin tile.", this);
                return false;
            }

            bool success = PerformBossMove(unit, origin, target, onVisualComplete);

            if (success)
                OnMovePerformed?.Invoke(unit, origin, target);

            return success;
        }

        private bool PerformBossMove(UnitController unit, Tile origin, Tile target, Action onVisualComplete)
        {
            if (unit == null)
            {
                CustomLogger.LogWarning("Null unit in TryPerformBossMove.", this);
                return false;
            }

            if (target == null)
            {
                CustomLogger.LogWarning("Null target tile in TryPerformBossMove.", this);
                return false;
            }

            if (origin == null)
            {
                CustomLogger.LogWarning("Unit has no origin tile.", this);
                return false;
            }

            if (target == origin)
            {
                CustomLogger.LogWarning("Target tile is the same as origin tile.", this);
                return false;
            }

            if (unit.Model == null)
            {
                CustomLogger.LogWarning("Unit has no model.", this);
                return false;
            }

            if (unit.Model.RemainingMoves <= 0)
            {
                CustomLogger.LogWarning("Unit has no moves left.", this);
                unit.AttachToTile(origin);
                return false;
            }

            if (target.IsOccupied())
            {
                UnitController defender = target.OccupyingUnit;
                if (defender == null)
                {
                    CustomLogger.LogWarning($"Target tile at ({GridCoordinateFormatter.ToA1(target.Row, target.Column)})" +
                                            " is occupied but has no unit.", this);
                    return false;
                }

                if (defender.Team == unit.Team)
                {
                    CustomLogger.LogWarning("Attempted to attack same team unit at " +
                                            $"({GridCoordinateFormatter.ToA1(target.Row, target.Column)}).", this);
                    return false;
                }

                if (!defender.CanBeAttacked())
                {
                    CustomLogger.LogWarning($"Defender at ({GridCoordinateFormatter.ToA1(target.Row, target.Column)})" +
                                            " cannot be attacked.", this);
                    return false;
                }

                defender.ApplyDamage(unit.Model.GetFinalDamage(), unit);
                if (defender.Model.IsAlive)
                {
                    CustomLogger.Log($"Defender at ({GridCoordinateFormatter.ToA1(target.Row, target.Column)}) " +
                                     "survived the attack.", this);
                    return false;
                }
            }

            if (!unit.Model.IsAlive)
            {
                CustomLogger.Log($"Unit at ({GridCoordinateFormatter.ToA1(origin.Row, origin.Column)}) " +
                                        "died before completing its move.", this);
                return false;
            }

            origin.RemoveUnit(unit);
            target.PlaceUnit(unit);
            unit.AttachToTile(target, false);

            unit.Model.ConsumeMove();

            bool enteredLastRow = _board.IsLastRow(target.Row, unit.Team);

            bool lifetimeExpired = unit.Model.DecreaseLifetime();
            bool shouldDieAfterTween = enteredLastRow || lifetimeExpired;

            Vector3 targetPosition = target.transform.position;
            _unitTween.MoveUnitTo(unit, targetPosition, moveData: null, markUnitTransitioning: true,
                onComplete: FinishVisual);
            return true;

            void FinishVisual()
            {
                if (enteredLastRow)
                    OnUnitEnteredLastRow?.Invoke(unit);

                if (shouldDieAfterTween)
                    unit.Die();

                onVisualComplete?.Invoke();
            }
        }

        private void HandleOpeningSetupExecuted() => _shouldSkipTurn = true;
    }
}