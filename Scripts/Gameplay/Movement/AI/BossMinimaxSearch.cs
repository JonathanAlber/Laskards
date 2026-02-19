using System.Collections.Generic;
using System.Text;
using Gameplay.CardExecution;
using Gameplay.Movement.AI.Data;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Performs depth-limited minimax search with alpha-beta pruning.
    /// The boss is always the maximizing player.
    /// The player is always the minimizing player.
    /// Both sides may end their phase by issuing a pass action.
    /// </summary>
    public sealed class BossMinimaxSearch
    {
        private const int MaxSupportedDepth = 32;

        private readonly BossAiEvaluator _evaluator;
        private readonly AiMoveGenerator _moveGenerator;
        private readonly BossAiMoveOrderingSettings _orderSettings;

        private readonly FlattenedArray<AiMove> _killerMoves = new(MaxSupportedDepth, 2);
        private readonly Dictionary<AiMove, int> _historyScores = new();

        private int[] _nodesPerDepth;
        private int[] _evalsPerDepth;
        private int[] _movesPerDepthBoss;
        private int[] _movesPerDepthPlayer;

        private int _nodesVisited;
        private int _cutoffs;
        private int _childSkips;
        private int _maxDepthReached;
        private int _rootSearchDepth;

        public BossMinimaxSearch(BossAiEvaluator evaluator, AiMoveGenerator aiMoveGenerator,
            BossAiMoveOrderingSettings orderSettings)
        {
            _evaluator = evaluator;
            _moveGenerator = aiMoveGenerator;
            _orderSettings = orderSettings;
        }

        /// <summary>
        /// Evaluates a state for the given active team using a depth-limited minimax search
        /// with alpha-beta pruning, without returning a concrete move.
        /// Useful for things like card placement, where you only care about the score.
        /// </summary>
        /// <param name="state">The state to evaluate.</param>
        /// <param name="depth">Maximum search depth.</param>
        /// <param name="activeTeam">Team whose phase is considered active at the root.</param>
        /// <returns>A score from the boss perspective (higher is better for the boss).</returns>
        public float EvaluateStateWithSearch(AiGameState state, int depth, ETeam activeTeam)
        {
            if (depth <= 0)
                return _evaluator.Evaluate(state, depth);

            ResetDebugCounters();
            ResetSearchHeuristics();

            _rootSearchDepth = depth;
            int length = depth + 1;
            _nodesPerDepth = new int[length];
            _evalsPerDepth = new int[length];
            _movesPerDepthBoss = new int[length];
            _movesPerDepthPlayer = new int[length];
            _maxDepthReached = 0;

            const float alpha = float.NegativeInfinity;
            const float beta = float.PositiveInfinity;

            return Search(state, depth, alpha, beta, activeTeam);
        }

        /// <summary>
        /// Searches for the best immediate boss move from the given state.
        /// The search simulates both boss and player responses.
        /// </summary>
        /// <param name="state">Root state at start of boss phase.</param>
        /// <param name="depth">Maximum search depth.</param>
        /// <param name="bestMove">Best boss move found.</param>
        /// <returns><c>true</c> when at least one move (or pass) is evaluated.</returns>
        public bool TryFindBestMove(AiGameState state, int depth, out AiMove bestMove)
        {
            bestMove = AiMove.CreatePass();

            if (depth <= 0)
                return false;

            ResetDebugCounters();
            ResetSearchHeuristics();

            _rootSearchDepth = depth;
            int length = depth + 1;
            _nodesPerDepth = new int[length];
            _evalsPerDepth = new int[length];
            _movesPerDepthBoss = new int[length];
            _movesPerDepthPlayer = new int[length];
            _maxDepthReached = 0;

            const float beta = float.PositiveInfinity;
            float alpha = float.NegativeInfinity;

            List<AiMove> moves = _moveGenerator.GenerateMoves(state, ETeam.Boss);
            bool hasRealMoves = moves.Count > 0;

            // Evaluate pass (phase end)
            AiGameState passState = AiPhaseTransitionUtility.BeginPhase(state, ETeam.Player);
            float passScore = Search(passState, depth - 1, alpha, beta, ETeam.Player);

            float bestValue = passScore;
            AiMove bestFound = AiMove.CreatePass();
            alpha = bestValue;

            // Evaluate real moves
            const int depthIndexRoot = 0; // children of root are considered depth 0 for stats
            int explored = 0;

            ReorderMoves(moves, depthIndexRoot);

            foreach (AiMove move in moves)
            {
                explored++;

                AiGameState next = state.ApplyMove(move, ETeam.Boss);
                float score = Search(next, depth - 1, alpha, beta, ETeam.Boss);

                if (score > bestValue)
                {
                    bestValue = score;
                    bestFound = move;
                }

                if (score > alpha)
                    alpha = score;

                if (!(alpha >= beta))
                    continue;

                _cutoffs++;
                _childSkips += moves.Count - explored;
                RegisterKiller(move, depthIndexRoot);
                UpdateHistory(move, depth - 1);
                break;
            }

            if (!hasRealMoves && bestFound.IsPass)
                return false;

            bestMove = bestFound;

            CustomLogger.Log(
                $"Best move value {bestValue} after visiting {_nodesVisited} nodes, " +
                $"{_cutoffs} cutoffs, skipping {_childSkips} child nodes.\n" +
                $"MaxDepthReached={_maxDepthReached} (0=root children)\n" +
                BuildDepthStatsString(),
                null);

            return true;
        }

        private bool IsTerminal(AiGameState state)
        {
            if (state.PlayerHp <= 0)
                return true;

            List<AiMove> bossMoves = _moveGenerator.GenerateMoves(state, ETeam.Boss);
            if (bossMoves.Count > 0)
                return false;

            List<AiMove> playerMoves = _moveGenerator.GenerateMoves(state, ETeam.Player);
            return playerMoves.Count == 0;
        }

        private float Search(AiGameState state, int depth, float alpha, float beta, ETeam activeTeam)
        {
            _nodesVisited++;

            int d = GetDepthFromRoot(depth);
            if (d < 0)
                d = 0;
            if (d >= _nodesPerDepth.Length)
                d = _nodesPerDepth.Length - 1;

            // Depth index for killer/history arrays (clamped separately)
            int depthIndex = d;
            if (depthIndex < 0)
                depthIndex = 0;
            if (depthIndex >= MaxSupportedDepth)
                depthIndex = MaxSupportedDepth - 1;

            _nodesPerDepth[d]++;
            if (d > _maxDepthReached) _maxDepthReached = d;

            if (depth == 0 || IsTerminal(state))
            {
                _evalsPerDepth[d]++;
                return _evaluator.Evaluate(state, depth);
            }

            bool maximizing = activeTeam == ETeam.Boss;

            List<AiMove> moves = _moveGenerator.GenerateMoves(state, activeTeam);

            if (maximizing)
                _movesPerDepthBoss[d] += moves.Count;
            else
                _movesPerDepthPlayer[d] += moves.Count;

            ETeam opponent = activeTeam == ETeam.Boss ? ETeam.Player : ETeam.Boss;
            float value = maximizing ? float.NegativeInfinity : float.PositiveInfinity;

            AiGameState passState = AiPhaseTransitionUtility.BeginPhase(state, opponent);
            float passScore = Search(passState, depth - 1, alpha, beta, opponent);

            if (maximizing)
            {
                if (passScore > value)
                    value = passScore;

                if (value > alpha)
                    alpha = value;

                if (alpha >= beta)
                {
                    _cutoffs++;
                    return value;
                }

                int explored = 0;

                ReorderMoves(moves, depthIndex);

                foreach (AiMove move in moves)
                {
                    explored++;

                    AiGameState child = state.ApplyMove(move, activeTeam);
                    float score = Search(child, depth - 1, alpha, beta, activeTeam);

                    if (score > value)
                        value = score;
                    if (score > alpha)
                        alpha = score;

                    if (!(alpha >= beta))
                        continue;

                    _cutoffs++;
                    _childSkips += moves.Count - explored;
                    RegisterKiller(move, depthIndex);
                    UpdateHistory(move, depth - 1);
                    break;
                }
            }
            else
            {
                if (passScore < value)
                    value = passScore;

                if (value < beta)
                    beta = value;

                if (alpha >= beta)
                {
                    _cutoffs++;
                    return value;
                }

                int explored = 0;

                ReorderMoves(moves, depthIndex);

                foreach (AiMove move in moves)
                {
                    explored++;

                    AiGameState child = state.ApplyMove(move, activeTeam);
                    float score = Search(child, depth - 1, alpha, beta, activeTeam);

                    if (score < value)
                        value = score;
                    if (score < beta)
                        beta = score;

                    if (!(alpha >= beta))
                        continue;

                    _cutoffs++;
                    _childSkips += moves.Count - explored;
                    RegisterKiller(move, depthIndex);
                    UpdateHistory(move, depth - 1);
                    break;
                }
            }

            return value;
        }

        private void ReorderMoves(List<AiMove> moves, int depthIndex)
        {
            if (moves is not { Count: > 1 })
                return;

            moves.Sort((a, b) =>
            {
                int scoreA = GetMoveOrderScore(a, depthIndex);
                int scoreB = GetMoveOrderScore(b, depthIndex);
                return scoreB.CompareTo(scoreA); // descending
            });
        }

        private int GetMoveOrderScore(AiMove move, int depthIndex)
        {
            if (move.IsPass)
                return 0;

            int score = 0;

            AiMove k1 = _killerMoves[depthIndex, 0];
            AiMove k2 = _killerMoves[depthIndex, 1];

            if (move.Equals(k1))
                score += _orderSettings.KillerPrimaryScore;
            else if (move.Equals(k2))
                score += _orderSettings.KillerSecondaryScore;

            if (move.IsCapture)
                score += _orderSettings.CaptureScore;

            if (_historyScores.TryGetValue(move, out int hist))
                score += hist;

            score += (int)(move.HeuristicDelta * _orderSettings.HeuristicDeltaMultiplier);
            score += move.ForwardDelta * _orderSettings.ForwardDeltaMultiplier;

            return score;
        }

        private void RegisterKiller(AiMove move, int depthIndex)
        {
            // Classic killer heuristic usually only uses non-captures
            if (move.IsPass || move.IsCapture)
                return;

            AiMove existing0 = _killerMoves[depthIndex, 0];

            if (existing0.Equals(move))
                return;

            _killerMoves[depthIndex, 1] = existing0;
            _killerMoves[depthIndex, 0] = move;
        }

        private void UpdateHistory(AiMove move, int depth)
        {
            // Only track non-capturing, non-pass moves
            if (move.IsPass || move.IsCapture)
                return;

            int current = _historyScores.GetValueOrDefault(move, 0);

            // Deeper in the tree = slightly higher bonus
            int bonus = depth * depth + 1;
            _historyScores[move] = current + bonus;
        }

        private void ResetDebugCounters()
        {
            _nodesVisited = 0;
            _cutoffs = 0;
            _childSkips = 0;
        }

        private void ResetSearchHeuristics()
        {
            for (int d = 0; d < MaxSupportedDepth; d++)
            {
                _killerMoves[d, 0] = AiMove.CreatePass();
                _killerMoves[d, 1] = AiMove.CreatePass();
            }

            _historyScores.Clear();
        }

        private string BuildDepthStatsString()
        {
            StringBuilder sb = new();

            sb.AppendLine("Depth stats:");
            for (int d = 0; d < _nodesPerDepth.Length; d++)
            {
                sb.AppendLine(
                    $"  d={d}: nodes={_nodesPerDepth[d]}, evals={_evalsPerDepth[d]}, " +
                    $"bossMoves={_movesPerDepthBoss[d]}, playerMoves={_movesPerDepthPlayer[d]}");
            }

            return sb.ToString();
        }

        private int GetDepthFromRoot(int remainingDepth) => _rootSearchDepth - remainingDepth;
    }
}